using System.Collections.Concurrent;
using Reflect.Domain.Entities;

namespace Reflect.Web.Services;

// Demo-only in-memory acknowledgments; keyed by clinician, patient and signal.
public sealed class AttentionAcknowledgementService
{
    private readonly ConcurrentDictionary<(Guid Clinician, Guid Patient, string Signal), DateTimeOffset> _acknowledged = new();

    public void Acknowledge(Guid clinicianId, Guid patientId, string signal, DateTimeOffset latestSessionAt)
    {
        _acknowledged[(clinicianId, patientId, signal)] = latestSessionAt;
    }

    public bool IsAcknowledged(Guid clinicianId, Guid patientId, string signal, DateTimeOffset latestSessionAt)
    {
        return _acknowledged.TryGetValue((clinicianId, patientId, signal), out var recordedAt)
            && latestSessionAt <= recordedAt;
    }
}