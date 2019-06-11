using System.Drawing;

namespace DR.NummerStripper.MU
{
    public interface IResult
    {
        string ProductionNumber { get; }
        ProgramCard ProgramCard { get; }
        Image Image { get; }
    }
}