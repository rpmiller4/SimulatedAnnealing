namespace SimulatedAnnealing.Models
{
    public class Appointment
    {
        public int CityId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int VehicleId { get; set; }

        public Appointment Copy()
        {
            return new Appointment
            {
                CityId = CityId,
                Start = Start,
                End = End,
                VehicleId = VehicleId
            };
        }
    }
}
