using System.Threading.Tasks;

namespace BetaseriesStandardLib
{
    public interface IGuessIt
    {
        Task<string> GuessNom(string name);
    }
}