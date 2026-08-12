using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Abstractions.Errors;

public static class PollErrors
{
    // HTTP 404
    public static readonly Error NotFound =
        Error.NotFound("Polls.NotFound", "The requested poll was not found.");

    // HTTP 409
    public static readonly Error DuplicateTitle =
        Error.Conflict("Polls.DuplicateTitle", "A poll with the same title already exists.");

    // HTTP 400
    public static readonly Error InvalidDates =
        Error.BadRequest("Polls.InvalidDates", "The end date must be after the start date.");

    // HTTP 400
    public static readonly Error PastStartDate =
        Error.BadRequest("Polls.PastStartDate", "The start date cannot be set in the past.");

    // HTTP 409
    public static readonly Error AlreadyPublished =
        Error.Conflict("Polls.AlreadyPublished", "Published polls cannot be modified.");

    // HTTP 400
    public static readonly Error NotPublished =
        Error.BadRequest("Polls.NotPublished", "This poll is not published yet.");

    // HTTP 400
    public static readonly Error VotingClosed =
        Error.BadRequest("Polls.VotingClosed", "The voting period for this poll has ended.");

    // HTTP 409
    public static readonly Error AlreadyVoted =
        Error.Conflict("Polls.AlreadyVoted", "You have already submitted a vote for this poll.");
}