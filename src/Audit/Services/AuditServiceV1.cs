
using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories;
using AyBorg.Data.Audit.Repositories.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Audit.Services;

public sealed class AuditServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    private readonly ILogger<AuditServiceV1> _logger;
    private readonly IAgentAuditService _agentAuditService;
    private readonly IProjectAuditRepository _projectAuditRepository;
    private readonly IAuditReportRepository _auditReportRepository;

    public AuditServiceV1(ILogger<AuditServiceV1> logger, IAgentAuditService agentAuditService, IProjectAuditRepository projectAuditRepository, IAuditReportRepository auditReportRepository)
    {
        _logger = logger;
        _agentAuditService = agentAuditService;
        _projectAuditRepository = projectAuditRepository;
        _auditReportRepository = auditReportRepository;
    }

    public override Task<Empty> AddEntry(AuditEntry request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            switch (request.PayloadCase)
            {
                case AuditEntry.PayloadOneofCase.AgentProject:
                    ProjectAuditRecord projectAuditRecord = AuditMapper.Map(request);
                    if (!_agentAuditService.TryAdd(projectAuditRecord))
                    {
                        throw new RpcException(new Status(StatusCode.DataLoss, "Failed to add project audit entry."));
                    }
                    break;
            }

            return new Empty();
        });
    }

    public override Task<Empty> InvalidateEntry(InvalidateAuditEntryRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            if (request.ServiceType.Equals(ServiceTypes.Agent))
            {
                if (!_agentAuditService.TryRemove(Guid.Parse(request.Token)))
                {
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to invalidate audit entry."));
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid service type."));
            }

            return new Empty();
        });
    }

    public override async Task GetChangesets(GetAuditChangesetsRequest request, IServerStreamWriter<AuditChangeset> responseStream, ServerCallContext context)
    {
        foreach (ChangesetRecord changeset in _agentAuditService.GetChangesets())
        {
            await responseStream.WriteAsync(AuditMapper.Map(changeset));
        }
    }

    public override async Task GetChanges(GetAuditChangesRequest request, IServerStreamWriter<AuditChange> responseStream, ServerCallContext context)
    {
        IEnumerable<ProjectAuditRecord> changesets = _projectAuditRepository.FindAll().Where(c => request.Tokens.Any(t => t.Equals(c.Id.ToString(), StringComparison.OrdinalIgnoreCase)));
        if (!changesets.Any())
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "No changesets found."));
        }

        IEnumerable<ChangeRecord> changes = AgentCompareService.Compare(changesets);
        foreach (ChangeRecord change in changes)
        {
            await responseStream.WriteAsync(AuditMapper.Map(change));
        }
    }

    public override Task<Empty> AddReport(AddAuditReportRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var record = new AuditReportRecord
            {
                Name = request.ReportName,
                Comment = request.Comment
            };

            IEnumerable<ProjectAuditRecord> requestedChangesets = _projectAuditRepository.FindAll().Where(c => request.Changesets.Any(r => r.Token.Equals(c.Id.ToString())));
            if (!requestedChangesets.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "No changesets found."));
            }

            record.Changesets.AddRange(requestedChangesets);

            if (!_auditReportRepository.TryAdd(record))
            {
                throw new RpcException(new Status(StatusCode.DataLoss, "Failed to save audit report."));
            }

            _logger.LogInformation(new EventId((int)EventLogType.Audit), "Audit report [{name}] saved.", record.Name);

            return new Empty();
        });
    }

    public override Task<Empty> DeleteReport(AuditReport request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            AuditReportRecord report = AuditMapper.Map(request);
            if (!_auditReportRepository.TryRemove(report))
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to delete audit report."));
            }
            return new Empty();
        });
    }

    public override async Task GetReports(Empty request, IServerStreamWriter<AuditReport> responseStream, ServerCallContext context)
    {
        foreach (AuditReportRecord report in _auditReportRepository.FindAll())
        {
            await responseStream.WriteAsync(AuditMapper.Map(report));
        }
    }
}