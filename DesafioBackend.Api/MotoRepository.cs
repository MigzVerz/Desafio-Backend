using DesafioBackend.Api.Models;

namespace DefaultNamespace;

public class MotoRepository
{
    public class Repository
    {
        private static readonly List<Motorcycle> _motos = new();
        private static int _idCounter = 1;

        public IEnumerable<Motorcycle> GetAll() => _motos;

        public Motorcycle Add(Motorcycle moto)
        {
            moto.Id = _idCounter++;
            _motos.Add(moto);
            return moto;
        }
    }
}