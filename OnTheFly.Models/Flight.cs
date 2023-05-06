﻿namespace OnTheFly.Models
{
    public class Flight
    {
        public Airport Destiny { get; set; }
        public AirCraft Plane { get; set; }
        public int Sales { get; set; }
        public DateTime Departure { get; set; }
        public bool Status { get; set; }
    }
}