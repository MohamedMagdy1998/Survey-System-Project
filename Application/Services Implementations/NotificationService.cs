using Application.Helpers;
using Application.Services_Interfaces;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class NotificationService(
  IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender) : INotificationService
{
    private readonly IUnitOfWork UnitOfWork = unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollsNotification(int? pollId = null)
    {
        ICollection<Poll> polls = [];

        if (pollId.HasValue)
        {
            var poll = await UnitOfWork.Polls.GetAsync(pollId.Value);
            if (poll is not null && poll.IsPublished)
            {
                polls.Add(poll);
            }
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activePolls = await UnitOfWork.Polls.GetAllAsync();
            polls =  activePolls
                            .Where(x => x.IsPublished && x.StartsAt == today)
                            .ToList();
        }

        //TODO: Select members only
        var users = await _userManager.Users.ToListAsync();

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var poll in polls)
        {
            foreach (var user in users)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "{{name}}", user.FirstName },
                    { "{{pollTill}}", poll.Title },
                    { "{{endDate}}", poll.EndsAt.ToString() },
                    { "{{url}}", $"{origin}/polls/start/{poll.Id}" }
                };

                var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeholders);

                await _emailSender.SendEmailAsync(user.Email!, $"📣 Survey Basket: New Poll - {poll.Title}", body);
            }
        }
    }
}