using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OnTheFly.Models.DTO;

namespace OnTheFly.Models
{
    public class Passenger
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [StringLength(14)]
        public string CPF { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        [StringLength(1)]
        public string Gender { get; set; }
        [StringLength(14)]
        public string? Phone { get; set; }
        public DateTime DtBirth { get; set; }
        public DateTime DtRegister { get; set; }
        public bool Status { get; set; }
        public Address Address { get; set; }

        public bool ValidateCPF(string cpf)
        {

            bool status = false;

            Console.WriteLine(cpf);

            int count = 0;
            for (int i = 0; i < 11; i++)
            {
                if (cpf[0] == cpf[i])
                    count++;
            }
            if (count == 11) return false;

            int firstdigit = 0;
            for (int i = 0; i < 9; i++)
            {
                var digitcpf = int.Parse(cpf[i].ToString());
                firstdigit = firstdigit + (digitcpf * (10 - i));
            }


            int seconddigit = 0;
            for (int i = 0; i < 10; i++)
            {
                var digitcpf = int.Parse(cpf[i].ToString());
                seconddigit = seconddigit + (digitcpf * (11 - i));
            }

            var modfirst = (firstdigit * 10) % 11;
            var modsecond = (seconddigit * 10) % 11;

            if (modfirst == 10)
            {
                modfirst = 0;
            }
            if (modsecond == 10)
            {
                modsecond = 0;
            }
            if (modfirst == int.Parse(cpf[9].ToString()) &&
                modsecond == int.Parse(cpf[10].ToString()))
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }
        public bool ValidateAge(Passenger passenger)
        {
            return (DateTime.Now.Subtract(passenger.DtBirth).TotalDays >= 18 * 365) ? true : false;
        }
    }
}
