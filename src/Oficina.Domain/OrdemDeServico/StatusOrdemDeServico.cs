namespace Oficina.Domain.Atendimento;

/// <summary>Estados definidos pelo requisito RF10 da FIAP.</summary>
public enum StatusOrdemDeServico
{
    Recebida,
    EmDiagnostico,
    AguardandoAprovacao,
    EmExecucao,
    Finalizada,
    Entregue
}
