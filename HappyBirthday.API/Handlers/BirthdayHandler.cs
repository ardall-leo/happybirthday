using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models.Events;
using NServiceBus;
using System;
using System.Threading.Tasks;

namespace HappyBirthday.API.Handlers
{
    public class BirthdayHandler : IHandleMessages<BirthdayEvent>
    {
        private readonly IBirthdayService _service;

        public BirthdayHandler(IBirthdayService service)
        {
            _service = service;
        }

        public async Task Handle(BirthdayEvent message, IMessageHandlerContext context)
        {
            await _service.SendHappyBirthday(message.BirthdayUser);
        }
    }
}
