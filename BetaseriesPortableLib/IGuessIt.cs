using System.Threading.Tasks;

namespace BetaseriesPortableLib
{
    public interface IGuessIt
    {
        Task<string> GuessNom(string name);
    }
}