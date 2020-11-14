using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MicroservicesRabbitMQ.Domain.Core.Bus;
using MicroservicesRabbitMQ.Domain.Core.Commands;
using MicroservicesRabbitMQ.Domain.Core.Events;
using MediatR;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Linq;

namespace MicroservicesRabbitMQ.Infrastructure.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// Used for different Microservices to publish events to RabbitMQ Server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        public void Publish<T>(T @event) where T : Event
        {
            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            
            using (IConnection connection =connectionFactory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            {
                string eventName = @event.GetType().Name;

                channel.QueueDeclare(eventName, false, false, false, null);

                string message = JsonConvert.SerializeObject(@event);

                byte[] body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(string.Empty, eventName, null, body);
            }
        }        

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            Type @event = typeof(T);
            Type handlerType = typeof(TH);

            string eventName = @event.Name;
            
            if (!_eventTypes.Contains(handlerType))
                _eventTypes.Add(@event);

            if (!_handlers.ContainsKey(eventName))
                _handlers.Add(eventName, new List<Type>());

            if (_handlers[eventName].Any(item => item.GetType() == handlerType))
                throw new ArgumentException($"Handler Type {handlerType.Name} is already registered for '{eventName}'.", nameof(handlerType));

            _handlers[eventName].Add(handlerType);
        }
    }
}
