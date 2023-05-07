using OnTheFly.Models;
using OnTheFly.Connections;
using Newtonsoft.Json;
using OnTheFly.Models.DTO;

namespace OnTheFly.PassengerService.Services
{
    public class PassengerServices
    {
        private readonly PassengerConnection _passengerConnection;
        public PassengerServices(PassengerConnection passagenderConnection)
        {
            _passengerConnection = passagenderConnection;
        }
        public List<Passenger> GetAll()
        {
            return _passengerConnection.FindAll();

        }
        public Passenger GetBycpf(string cpf)
        {
            return _passengerConnection.FindPassenger(cpf);
        }
        public Passenger Insert(PassengerDTO passengerdto)
        {
            return _passengerConnection.Insert(passengerdto);
        }
        public bool Delete(string cpf)
        {
            bool status = false;
            try
            {
                _passengerConnection.Delete(cpf);
                status = true;

            }catch
            {
                status = false;
            }
            return status;
        }
        public Passenger Update(string cpf, Passenger passenger)
        {
            return _passengerConnection.Update(cpf, passenger); ;
        }
    }
}
