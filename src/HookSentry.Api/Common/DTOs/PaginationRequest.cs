namespace HookSentry.Api.Common.DTOs;

public enum SortOrder { Asc, Desc }

public class PaginationRequest
{
    public int Qt { get; set; } = 10;
    public int Pg { get; set; } = 1;
    public string CpOrd { get; set; } = "id";
    public SortOrder TpOrd { get; set; } = SortOrder.Desc;

    public PaginationRequest() { }

    public PaginationRequest(int qt = 10, int pg = 1, string cpOrd = "id", SortOrder tpOrd = SortOrder.Desc)
    {
        Qt = qt;
        Pg = pg;
        CpOrd = cpOrd;
        TpOrd = tpOrd;
    }
}
