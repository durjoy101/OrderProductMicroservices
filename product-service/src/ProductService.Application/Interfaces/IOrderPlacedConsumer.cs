using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    // Background consumer must be started by the host; interface allows mocking in tests
    public interface IOrderPlacedConsumer
    {
        void Start();   // start consuming in background
    }
}
