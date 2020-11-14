using MicroservicesRabbitMQ.Domain.Core.Commands;
using MicroservicesRabbitMQ.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicesRabbitMQ.Domain.Core.Bus
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;

        void Publish<T>(T @event) where T : Event;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH">Event Handler</typeparam>
        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;
    }
}
