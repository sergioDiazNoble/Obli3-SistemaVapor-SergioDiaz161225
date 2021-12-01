using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Data
{
    public class Logs<T>
    {
        private static int idCounter = 0;

        public Logs(int id)
        {
            Id = id;
        }

        public Logs() : this(++idCounter)
        {
            Type = typeof(T).ToString();
        }

        public Logs<Q> As<Q>() where Q : T
        {
            return new Logs<Q>(Id)
            {
                Description = Description,
                Date = Date,
                Object = JsonSerializer.Deserialize<Q>(Object.ToString())
            };
        }

        public T Object { get; set; }

        public string Description { get; set; }
        public string Type { get; }
        public DateTime Date { get; set; }
        public int Id { get; }
    }

}

