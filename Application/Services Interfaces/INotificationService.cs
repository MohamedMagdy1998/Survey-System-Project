using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Interfaces;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
