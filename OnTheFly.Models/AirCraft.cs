using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnTheFly.Models
{
    public class AirCraft
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [StringLength(6)]
        public string RAB { get; set; }
        public int Capacity { get; set; }
        public DateTime DtRegistry { get; set; }
        public DateTime? DtLastFlight { get; set; }
        public Company Company { get; set; }

        public static bool RABValidation(string rab)
        {
            rab=rab.ToLower();
            char[] aceptedLetters = new char[] { 'p', 'r', 's', 't', 'u' };
            string[] unaceptedPrefixes = new string[] { "sos", "xxx", "pan", "ttt", "vfr", "ifr", "vmc", "imc", "tnc", "pqp", "pnc"};

            StringBuilder aux1 = new StringBuilder();
            aux1.Append(rab[0]);
            aux1.Append(rab[1]);

            StringBuilder aux2 = new StringBuilder();
            aux2.Append(rab[2]);
            aux2.Append(rab[3]);
            aux2.Append(rab[4]);

            string pt1 = aux1.ToString();
            string pt2 = aux2.ToString();

            if (pt1[0] != 'p')
                return false;

            if(!aceptedLetters.Contains(pt1[1]))
                return false;

            if (pt2[0] == 'q' || pt2[1] == 'w')
                return false;

            if(unaceptedPrefixes.Contains(pt2))
                return false;

            if (rab.Equals("putas")) 
                return false;

            return true;
        }
    }
}
