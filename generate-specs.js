const fs = require('fs');
const path = require('path');

// 1. Build OpenAPI Specification
const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Survey System API",
    version: "1.0.0",
    description: "Complete API specification for the Survey System Project (Survey Basket). Includes Authentication, User Management, Role Management, Poll Lifecycle, Question & Answer Management, Voting, and Results Analytics.",
    contact: {
      name: "Survey System Team"
    }
  },
  servers: [
    {
      url: "http://localhost:5289",
      description: "Local HTTP server (Active)"
    },
    {
      url: "https://localhost:7108",
      description: "Local HTTPS server"
    }
  ],
  tags: [
    { name: "Auth", description: "Authentication, Registration, and Password Reset" },
    { name: "Account", description: "Current User Profile & Account Management" },
    { name: "Polls", description: "Poll Creation, Updating, Publishing, and Retrieval" },
    { name: "Questions", description: "Poll Questions and Answer Choices" },
    { name: "Votes", description: "Voting Operations for Members" },
    { name: "Results", description: "Poll Results, Analytics, and Voting Statistics" },
    { name: "Roles", description: "Role-Based Access Control and Permission Assignment" },
    { name: "Users", description: "User Account Administration" },
    { name: "Health", description: "Service Health Check" }
  ],
  paths: {
    "/auth/register": {
      post: {
        tags: ["Auth"],
        summary: "Register a new user",
        description: "Registers a new user account with first name, last name, email, and password.",
        operationId: "registerUser",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/RegisterRequest" },
              example: {
                email: "member@surveybasket.com",
                firstName: "John",
                lastName: "Doe",
                password: "Password123!"
              }
            }
          }
        },
        responses: {
          "200": { description: "User registered successfully" },
          "400": {
            description: "Bad Request - Validation or duplicate email failure",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "429": { description: "Too Many Requests (Rate limit exceeded)" }
        }
      }
    },
    "/auth/confirm-email": {
      post: {
        tags: ["Auth"],
        summary: "Confirm email address",
        description: "Confirms user registration using the emailed OTP / token code.",
        operationId: "confirmEmail",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ConfirmEmailRequest" },
              example: {
                userId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                code: "123456"
              }
            }
          }
        },
        responses: {
          "200": { description: "Email confirmed successfully" },
          "400": {
            description: "Invalid code or user",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth/resend-confirmation-email": {
      post: {
        tags: ["Auth"],
        summary: "Resend confirmation email",
        description: "Resends the confirmation code to the user's email address.",
        operationId: "resendConfirmationEmail",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ResendConfirmationEmailRequest" },
              example: {
                email: "member@surveybasket.com"
              }
            }
          }
        },
        responses: {
          "200": { description: "Confirmation email resent" },
          "400": {
            description: "User already confirmed or email not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth": {
      post: {
        tags: ["Auth"],
        summary: "User login",
        description: "Authenticates user credentials and returns JWT Bearer token and refresh token.",
        operationId: "login",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/LoginRequest" },
              example: {
                email: "admin@surveybasket.com",
                password: "Password123!"
              }
            }
          }
        },
        responses: {
          "200": {
            description: "Login successful",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/AuthResponse" }
              }
            }
          },
          "400": {
            description: "Invalid credentials or account disabled",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth/refresh": {
      post: {
        tags: ["Auth"],
        summary: "Refresh access token",
        description: "Generates a new access token and rotating refresh token using a valid unexpired refresh token.",
        operationId: "refreshToken",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/RefreshTokenRequest" },
              example: {
                token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                refreshToken: "dGhpcy1pcy1hLXJlZnJlc2gtdG9rZW4..."
              }
            }
          }
        },
        responses: {
          "200": {
            description: "Token refreshed successfully",
            content: { "application/json": { schema: { $ref: "#/components/schemas/AuthResponse" } } }
          },
          "400": {
            description: "Invalid or expired token",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth/revoke-refresh-token": {
      post: {
        tags: ["Auth"],
        summary: "Revoke refresh token",
        description: "Revokes the active refresh token to invalidate future refreshes.",
        operationId: "revokeRefreshToken",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/RefreshTokenRequest" },
              example: {
                token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                refreshToken: "dGhpcy1pcy1hLXJlZnJlc2gtdG9rZW4..."
              }
            }
          }
        },
        responses: {
          "200": { description: "Refresh token revoked successfully" },
          "400": {
            description: "Invalid refresh token",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth/forget-password": {
      post: {
        tags: ["Auth"],
        summary: "Forgot password request",
        description: "Sends a password reset code to the specified email.",
        operationId: "forgetPassword",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ForgetPasswordRequest" },
              example: {
                email: "member@surveybasket.com"
              }
            }
          }
        },
        responses: {
          "200": { description: "Password reset code sent if email exists" },
          "400": {
            description: "Invalid request",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/auth/reset-password": {
      post: {
        tags: ["Auth"],
        summary: "Reset password",
        description: "Resets the user password using the verification code received via email.",
        operationId: "resetPassword",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ResetPasswordRequest" },
              example: {
                email: "member@surveybasket.com",
                code: "123456",
                newPassword: "NewPassword123!"
              }
            }
          }
        },
        responses: {
          "200": { description: "Password reset successfully" },
          "400": {
            description: "Invalid code or password does not meet criteria",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          }
        }
      }
    },
    "/me": {
      get: {
        tags: ["Account"],
        summary: "Get current user profile",
        description: "Retrieves profile details for the authenticated user.",
        operationId: "getProfile",
        security: [{ BearerAuth: [] }],
        responses: {
          "200": {
            description: "Current user profile",
            content: { "application/json": { schema: { $ref: "#/components/schemas/UserProfileResponse" } } }
          },
          "401": { description: "Unauthorized" }
        }
      }
    },
    "/me/info": {
      put: {
        tags: ["Account"],
        summary: "Update current user profile",
        description: "Updates the authenticated user's first and last name.",
        operationId: "updateProfile",
        security: [{ BearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/UpdateProfileRequest" },
              example: {
                firstName: "John",
                lastName: "Smith"
              }
            }
          }
        },
        responses: {
          "204": { description: "Profile updated successfully" },
          "400": {
            description: "Validation failure",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" }
        }
      }
    },
    "/me/change-password": {
      put: {
        tags: ["Account"],
        summary: "Change current user password",
        description: "Changes the authenticated user's password after verifying the current password.",
        operationId: "changePassword",
        security: [{ BearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ChangePasswordRequest" },
              example: {
                currentPassword: "Password123!",
                newPassword: "NewSecurePassword123!"
              }
            }
          }
        },
        responses: {
          "204": { description: "Password changed successfully" },
          "400": {
            description: "Incorrect current password or new password invalid",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" }
        }
      }
    },
    "/api/polls": {
      get: {
        tags: ["Polls"],
        summary: "Get all polls",
        description: "Retrieves all polls (requires permission: `polls:read`).",
        operationId: "getAllPolls",
        security: [{ BearerAuth: [] }],
        responses: {
          "200": {
            description: "List of all polls",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/PollResponse" }
                }
              }
            }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden - missing permission polls:read" }
        }
      },
      post: {
        tags: ["Polls"],
        summary: "Create a new poll",
        description: "Creates a new poll (requires permission: `polls:add`).",
        operationId: "createPoll",
        security: [{ BearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/PollRequest" },
              example: {
                title: "Annual Employee Satisfaction Survey",
                summary: "Gather feedback regarding work environment, compensation, and leadership.",
                startsAt: "2026-10-10",
                endsAt: "2026-11-10"
              }
            }
          }
        },
        responses: {
          "201": {
            description: "Poll created successfully",
            content: { "application/json": { schema: { $ref: "#/components/schemas/PollResponse" } } }
          },
          "400": {
            description: "Validation error or duplicate poll title",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/current": {
      get: {
        tags: ["Polls"],
        summary: "Get current available polls",
        description: "Retrieves currently active and published polls for members (Role: `Member`).",
        operationId: "getCurrentPolls",
        security: [{ BearerAuth: [] }],
        responses: {
          "200": {
            description: "Active polls available for voting",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/PollResponse" }
                }
              }
            }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden - member role required" }
        }
      }
    },
    "/api/polls/{id}": {
      get: {
        tags: ["Polls"],
        summary: "Get poll by ID",
        description: "Retrieves a specific poll by ID (requires permission: `polls:read`).",
        operationId: "getPollById",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Poll details",
            content: { "application/json": { schema: { $ref: "#/components/schemas/PollResponse" } } }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      put: {
        tags: ["Polls"],
        summary: "Update an existing poll",
        description: "Updates poll title, summary, start date, and end date (requires permission: `polls:update`).",
        operationId: "updatePoll",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/PollRequest" },
              example: {
                title: "Updated Employee Satisfaction Survey",
                summary: "Updated survey regarding team alignment and work environment.",
                startsAt: "2026-10-10",
                endsAt: "2026-11-30"
              }
            }
          }
        },
        responses: {
          "204": { description: "Poll updated successfully" },
          "400": {
            description: "Validation error",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      delete: {
        tags: ["Polls"],
        summary: "Delete a poll",
        description: "Deletes a poll and its associated questions/votes (requires permission: `polls:delete`).",
        operationId: "deletePoll",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "204": { description: "Poll deleted successfully" },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/{id}/togglePublish": {
      put: {
        tags: ["Polls"],
        summary: "Toggle poll publish status",
        description: "Toggles whether a poll is published or unpublished (requires permission: `polls:update`).",
        operationId: "togglePublishPoll",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "204": { description: "Publish status toggled successfully" },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/{pollId}/questions": {
      get: {
        tags: ["Questions"],
        summary: "Get questions for a poll",
        description: "Retrieves paginated and filtered questions belonging to a poll (requires permission: `questions:read`).",
        operationId: "getPollQuestions",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          },
          {
            name: "pageNumber",
            in: "query",
            required: false,
            schema: { type: "integer", default: 1 },
            description: "Page number"
          },
          {
            name: "pageSize",
            in: "query",
            required: false,
            schema: { type: "integer", default: 10, maximum: 50 },
            description: "Number of items per page"
          },
          {
            name: "searchValue",
            in: "query",
            required: false,
            schema: { type: "string" },
            description: "Keyword to filter questions"
          },
          {
            name: "sortColumn",
            in: "query",
            required: false,
            schema: { type: "string" },
            description: "Column name to sort by"
          },
          {
            name: "sortDirection",
            in: "query",
            required: false,
            schema: { type: "string", default: "Asc", enum: ["Asc", "Desc"] },
            description: "Sort direction"
          }
        ],
        responses: {
          "200": {
            description: "Questions list",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/QuestionResponse" }
                }
              }
            }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      post: {
        tags: ["Questions"],
        summary: "Add a question to a poll",
        description: "Creates a new question with options for a poll (requires permission: `questions:add`).",
        operationId: "addQuestion",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/QuestionRequest" },
              example: {
                content: "How satisfied are you with our internal communication?",
                answers: ["Very Satisfied", "Satisfied", "Neutral", "Dissatisfied"]
              }
            }
          }
        },
        responses: {
          "201": {
            description: "Question created successfully",
            content: { "application/json": { schema: { $ref: "#/components/schemas/QuestionResponse" } } }
          },
          "400": {
            description: "Validation error or duplicate answers",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/{pollId}/questions/{Id}": {
      get: {
        tags: ["Questions"],
        summary: "Get question by ID",
        description: "Retrieves a single question by ID under a specific poll (requires permission: `questions:read`).",
        operationId: "getQuestionById",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          },
          {
            name: "Id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The question ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Question details",
            content: { "application/json": { schema: { $ref: "#/components/schemas/QuestionResponse" } } }
          },
          "404": {
            description: "Question or poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      put: {
        tags: ["Questions"],
        summary: "Update a question",
        description: "Updates question content and answer choices (requires permission: `questions:update`).",
        operationId: "updateQuestion",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          },
          {
            name: "Id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The question ID",
            example: 1
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/QuestionRequest" },
              example: {
                content: "How satisfied are you with work-life balance at the company?",
                answers: ["Excellent", "Good", "Moderate", "Needs Improvement"]
              }
            }
          }
        },
        responses: {
          "204": { description: "Question updated successfully" },
          "400": {
            description: "Validation error",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "404": {
            description: "Question or poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/{pollId}/questions/{Id}/toggleStatus": {
      patch: {
        tags: ["Questions"],
        summary: "Toggle question status",
        description: "Toggles question active status (requires permission: `questions:update`).",
        operationId: "toggleQuestionStatus",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          },
          {
            name: "Id",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The question ID",
            example: 1
          }
        ],
        responses: {
          "204": { description: "Question status toggled successfully" },
          "404": {
            description: "Question not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/polls/{pollId}/vote": {
      get: {
        tags: ["Votes"],
        summary: "Start vote / get available questions",
        description: "Retrieves questions and available answers for an active poll to allow a member to vote (Role: `Member`).",
        operationId: "startVote",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Available questions for voting",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/QuestionResponse" }
                }
              }
            }
          },
          "400": {
            description: "User already voted or poll is not active",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden - member role required" }
        }
      },
      post: {
        tags: ["Votes"],
        summary: "Submit vote",
        description: "Submits answers for a poll by an authenticated member (Role: `Member`).",
        operationId: "submitVote",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "path",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoteRequest" },
              example: {
                answers: [
                  { questionId: 1, answerId: 1 },
                  { questionId: 2, answerId: 3 }
                ]
              }
            }
          }
        },
        responses: {
          "201": { description: "Vote submitted successfully" },
          "400": {
            description: "Validation error or user already voted",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/results/row-data": {
      get: {
        tags: ["Results"],
        summary: "Get raw voting results",
        description: "Retrieves raw vote responses and voter selections for a poll (requires permission: `results:read`).",
        operationId: "getPollVotesRawData",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "query",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Raw poll votes data",
            content: { "application/json": { schema: { $ref: "#/components/schemas/PollVotesResponse" } } }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/results/votes-per-day": {
      get: {
        tags: ["Results"],
        summary: "Get votes per day",
        description: "Retrieves aggregated daily vote counts for a poll (requires permission: `results:read`).",
        operationId: "getVotesPerDay",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "query",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Daily votes breakdown",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/VotesPerDayResponse" }
                }
              }
            }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/results/votes-per-question": {
      get: {
        tags: ["Results"],
        summary: "Get votes per question",
        description: "Retrieves aggregated vote distribution across question options for a poll (requires permission: `results:read`).",
        operationId: "getVotesPerQuestion",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "pollId",
            in: "query",
            required: true,
            schema: { type: "integer" },
            description: "The poll ID",
            example: 1
          }
        ],
        responses: {
          "200": {
            description: "Votes per question breakdown",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/VotesPerQuestionResponse" }
                }
              }
            }
          },
          "404": {
            description: "Poll not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/roles": {
      get: {
        tags: ["Roles"],
        summary: "Get all roles",
        description: "Retrieves all user roles in the system (requires permission: `roles:read`).",
        operationId: "getAllRoles",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "includeDisabled",
            in: "query",
            required: false,
            schema: { type: "boolean", default: false },
            description: "Include soft-deleted/disabled roles"
          }
        ],
        responses: {
          "200": {
            description: "List of roles",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/RoleResponse" }
                }
              }
            }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      post: {
        tags: ["Roles"],
        summary: "Create a new role",
        description: "Creates a new custom role with designated permissions (requires permission: `roles:add`).",
        operationId: "createRole",
        security: [{ BearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/RoleRequest" },
              example: {
                name: "SurveyManager",
                permissions: [
                  "polls:read",
                  "polls:add",
                  "polls:update",
                  "questions:read",
                  "questions:add",
                  "questions:update",
                  "results:read"
                ]
              }
            }
          }
        },
        responses: {
          "201": {
            description: "Role created successfully",
            content: { "application/json": { schema: { $ref: "#/components/schemas/RoleDetailResponse" } } }
          },
          "400": {
            description: "Validation error or duplicate role name",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/roles/{id}": {
      get: {
        tags: ["Roles"],
        summary: "Get role by ID",
        description: "Retrieves details and assigned permissions for a role (requires permission: `roles:read`).",
        operationId: "getRoleById",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The role ID",
            example: "92b75286-d8f8-4061-9995-e6e23ccdee94"
          }
        ],
        responses: {
          "200": {
            description: "Role details with permissions",
            content: { "application/json": { schema: { $ref: "#/components/schemas/RoleDetailResponse" } } }
          },
          "404": {
            description: "Role not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      put: {
        tags: ["Roles"],
        summary: "Update role",
        description: "Updates role name and permissions (requires permission: `roles:update`).",
        operationId: "updateRole",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The role ID",
            example: "92b75286-d8f8-4061-9995-e6e23ccdee94"
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/RoleRequest" },
              example: {
                name: "SeniorSurveyManager",
                permissions: [
                  "polls:read",
                  "polls:add",
                  "polls:update",
                  "polls:delete",
                  "questions:read",
                  "questions:add",
                  "questions:update",
                  "results:read"
                ]
              }
            }
          }
        },
        responses: {
          "204": { description: "Role updated successfully" },
          "400": {
            description: "Validation error or invalid permissions",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "404": {
            description: "Role not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/roles/{id}/toggle-status": {
      put: {
        tags: ["Roles"],
        summary: "Toggle role status",
        description: "Toggles role active/deleted status (requires permission: `roles:update`).",
        operationId: "toggleRoleStatus",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The role ID",
            example: "92b75286-d8f8-4061-9995-e6e23ccdee94"
          }
        ],
        responses: {
          "204": { description: "Role status toggled successfully" },
          "404": {
            description: "Role not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/users": {
      get: {
        tags: ["Users"],
        summary: "Get all users",
        description: "Retrieves list of all users and their assigned roles (requires permission: `users:read`).",
        operationId: "getAllUsers",
        security: [{ BearerAuth: [] }],
        responses: {
          "200": {
            description: "List of users",
            content: {
              "application/json": {
                schema: {
                  type: "array",
                  items: { $ref: "#/components/schemas/UserResponse" }
                }
              }
            }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      post: {
        tags: ["Users"],
        summary: "Create a new user",
        description: "Creates a user account directly by an administrator (requires permission: `users:add`).",
        operationId: "createUser",
        security: [{ BearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CreateUserRequest" },
              example: {
                firstName: "Emily",
                lastName: "Clark",
                email: "emily.clark@surveybasket.com",
                password: "Password123!",
                roles: ["Member"]
              }
            }
          }
        },
        responses: {
          "201": {
            description: "User created successfully",
            content: { "application/json": { schema: { $ref: "#/components/schemas/UserResponse" } } }
          },
          "400": {
            description: "Validation error or email already in use",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/users/{id}": {
      get: {
        tags: ["Users"],
        summary: "Get user by ID",
        description: "Retrieves user account details by user ID (requires permission: `users:read`).",
        operationId: "getUserById",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The user ID",
            example: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
          }
        ],
        responses: {
          "200": {
            description: "User details",
            content: { "application/json": { schema: { $ref: "#/components/schemas/UserResponse" } } }
          },
          "404": {
            description: "User not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      },
      put: {
        tags: ["Users"],
        summary: "Update user",
        description: "Updates user basic details and role assignments (requires permission: `users:update`).",
        operationId: "updateUser",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The user ID",
            example: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
          }
        ],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/UpdateUserRequest" },
              example: {
                firstName: "Emily",
                lastName: "Johnson",
                email: "emily.johnson@surveybasket.com",
                roles: ["Member", "Admin"]
              }
            }
          }
        },
        responses: {
          "204": { description: "User updated successfully" },
          "400": {
            description: "Validation error or duplicate email",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "404": {
            description: "User not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/users/{id}/toggle-status": {
      put: {
        tags: ["Users"],
        summary: "Toggle user active/disabled status",
        description: "Toggles whether a user account is active or disabled (requires permission: `users:update`).",
        operationId: "toggleUserStatus",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The user ID",
            example: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
          }
        ],
        responses: {
          "204": { description: "User status toggled successfully" },
          "404": {
            description: "User not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/api/users/{id}/unlock": {
      put: {
        tags: ["Users"],
        summary: "Unlock user account",
        description: "Unlocks a locked user account (e.g. after multiple failed logins) (requires permission: `users:update`).",
        operationId: "unlockUser",
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "id",
            in: "path",
            required: true,
            schema: { type: "string" },
            description: "The user ID",
            example: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
          }
        ],
        responses: {
          "204": { description: "User account unlocked successfully" },
          "404": {
            description: "User not found",
            content: { "application/json": { schema: { $ref: "#/components/schemas/ProblemDetails" } } }
          },
          "401": { description: "Unauthorized" },
          "403": { description: "Forbidden" }
        }
      }
    },
    "/health": {
      get: {
        tags: ["Health"],
        summary: "Health Check",
        description: "Returns health status of the API and its dependent databases/services.",
        operationId: "getHealthStatus",
        responses: {
          "200": {
            description: "API is healthy",
            content: {
              "application/json": {
                schema: {
                  type: "object",
                  properties: {
                    status: { type: "string", example: "Healthy" },
                    totalDuration: { type: "string", example: "00:00:00.0125432" },
                    entries: { type: "object" }
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  components: {
    securitySchemes: {
      BearerAuth: {
        type: "http",
        scheme: "bearer",
        bearerFormat: "JWT",
        description: "Enter your JWT token obtained from the `/auth` endpoint."
      }
    },
    schemas: {
      RegisterRequest: {
        type: "object",
        required: ["email", "firstName", "lastName", "password"],
        properties: {
          email: { type: "string", format: "email", example: "member@surveybasket.com" },
          firstName: { type: "string", minLength: 3, maxLength: 100, example: "John" },
          lastName: { type: "string", minLength: 3, maxLength: 100, example: "Doe" },
          password: {
            type: "string",
            format: "password",
            minLength: 8,
            description: "Minimum 8 characters with at least 1 uppercase, 1 lowercase, 1 number, and 1 non-alphanumeric character.",
            example: "Password123!"
          }
        }
      },
      ConfirmEmailRequest: {
        type: "object",
        required: ["userId", "code"],
        properties: {
          userId: { type: "string", example: "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
          code: { type: "string", example: "123456" }
        }
      },
      ResendConfirmationEmailRequest: {
        type: "object",
        required: ["email"],
        properties: {
          email: { type: "string", format: "email", example: "member@surveybasket.com" }
        }
      },
      LoginRequest: {
        type: "object",
        required: ["email", "password"],
        properties: {
          email: { type: "string", format: "email", example: "admin@surveybasket.com" },
          password: { type: "string", format: "password", example: "Password123!" }
        }
      },
      AuthResponse: {
        type: "object",
        properties: {
          id: { type: "string", example: "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
          email: { type: "string", format: "email", example: "admin@surveybasket.com" },
          firstName: { type: "string", example: "Mohamed" },
          lastName: { type: "string", example: "Magdy" },
          token: { type: "string", example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." },
          expiresIn: { type: "integer", example: 1800, description: "Token lifetime in seconds" },
          refreshToken: { type: "string", example: "dGhpcy1pcy1hLXJlZnJlc2gtdG9rZW4..." },
          refreshTokenExpiryDate: { type: "string", format: "date-time", example: "2026-10-17T18:00:00Z" }
        }
      },
      RefreshTokenRequest: {
        type: "object",
        required: ["token", "refreshToken"],
        properties: {
          token: { type: "string", example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." },
          refreshToken: { type: "string", example: "dGhpcy1pcy1hLXJlZnJlc2gtdG9rZW4..." }
        }
      },
      ForgetPasswordRequest: {
        type: "object",
        required: ["email"],
        properties: {
          email: { type: "string", format: "email", example: "member@surveybasket.com" }
        }
      },
      ResetPasswordRequest: {
        type: "object",
        required: ["email", "code", "newPassword"],
        properties: {
          email: { type: "string", format: "email", example: "member@surveybasket.com" },
          code: { type: "string", example: "123456" },
          newPassword: { type: "string", format: "password", minLength: 8, example: "NewPassword123!" }
        }
      },
      UserProfileResponse: {
        type: "object",
        properties: {
          email: { type: "string", format: "email", example: "admin@surveybasket.com" },
          userName: { type: "string", example: "admin@surveybasket.com" },
          firstName: { type: "string", example: "Mohamed" },
          lastName: { type: "string", example: "Magdy" }
        }
      },
      UpdateProfileRequest: {
        type: "object",
        required: ["firstName", "lastName"],
        properties: {
          firstName: { type: "string", minLength: 3, maxLength: 100, example: "Mohamed" },
          lastName: { type: "string", minLength: 3, maxLength: 100, example: "El Helaly" }
        }
      },
      ChangePasswordRequest: {
        type: "object",
        required: ["currentPassword", "newPassword"],
        properties: {
          currentPassword: { type: "string", example: "OldPassword123!" },
          newPassword: { type: "string", minLength: 8, example: "NewSecurePassword123!" }
        }
      },
      PollRequest: {
        type: "object",
        required: ["title", "summary", "startsAt", "endsAt"],
        properties: {
          title: { type: "string", minLength: 3, maxLength: 100, example: "Customer Experience Survey 2026" },
          summary: { type: "string", minLength: 3, maxLength: 1500, example: "Comprehensive feedback on platform performance and customer delight." },
          startsAt: { type: "string", format: "date", example: "2026-10-05" },
          endsAt: { type: "string", format: "date", example: "2026-11-05" }
        }
      },
      PollResponse: {
        type: "object",
        properties: {
          id: { type: "integer", example: 1 },
          title: { type: "string", example: "Customer Experience Survey 2026" },
          summary: { type: "string", example: "Comprehensive feedback on platform performance." },
          isPublished: { type: "boolean", example: true },
          startsAt: { type: "string", format: "date", example: "2026-10-05" },
          endsAt: { type: "string", format: "date", example: "2026-11-05" }
        }
      },
      AnswerResponse: {
        type: "object",
        properties: {
          id: { type: "integer", example: 1 },
          content: { type: "string", example: "Highly Satisfied" }
        }
      },
      QuestionRequest: {
        type: "object",
        required: ["content", "answers"],
        properties: {
          content: { type: "string", minLength: 3, maxLength: 1000, example: "How satisfied are you with our responsiveness?" },
          answers: {
            type: "array",
            items: { type: "string" },
            minItems: 2,
            example: ["Highly Satisfied", "Satisfied", "Neutral", "Unsatisfied"]
          }
        }
      },
      QuestionResponse: {
        type: "object",
        properties: {
          id: { type: "integer", example: 1 },
          content: { type: "string", example: "How satisfied are you with our responsiveness?" },
          answers: {
            type: "array",
            items: { $ref: "#/components/schemas/AnswerResponse" }
          }
        }
      },
      VoteAnswerRequest: {
        type: "object",
        required: ["questionId", "answerId"],
        properties: {
          questionId: { type: "integer", minimum: 1, example: 1 },
          answerId: { type: "integer", minimum: 1, example: 1 }
        }
      },
      VoteRequest: {
        type: "object",
        required: ["answers"],
        properties: {
          answers: {
            type: "array",
            items: { $ref: "#/components/schemas/VoteAnswerRequest" }
          }
        }
      },
      QuestionAnswerResponse: {
        type: "object",
        properties: {
          question: { type: "string", example: "Overall Service Quality" },
          answer: { type: "string", example: "Excellent" }
        }
      },
      VoteResponse: {
        type: "object",
        properties: {
          voterName: { type: "string", example: "John Doe" },
          voteDate: { type: "string", format: "date-time", example: "2026-10-03T18:30:00Z" },
          selectedAnswers: {
            type: "array",
            items: { $ref: "#/components/schemas/QuestionAnswerResponse" }
          }
        }
      },
      PollVotesResponse: {
        type: "object",
        properties: {
          title: { type: "string", example: "Customer Experience Survey 2026" },
          votes: {
            type: "array",
            items: { $ref: "#/components/schemas/VoteResponse" }
          }
        }
      },
      VotesPerAnswerResponse: {
        type: "object",
        properties: {
          answer: { type: "string", example: "Excellent" },
          count: { type: "integer", example: 42 }
        }
      },
      VotesPerQuestionResponse: {
        type: "object",
        properties: {
          question: { type: "string", example: "How satisfied are you with our responsiveness?" },
          selectedAnswers: {
            type: "array",
            items: { $ref: "#/components/schemas/VotesPerAnswerResponse" }
          }
        }
      },
      VotesPerDayResponse: {
        type: "object",
        properties: {
          date: { type: "string", format: "date", example: "2026-10-03" },
          numberOfVotes: { type: "integer", example: 128 }
        }
      },
      RoleRequest: {
        type: "object",
        required: ["name", "permissions"],
        properties: {
          name: { type: "string", minLength: 3, maxLength: 200, example: "SurveyReviewer" },
          permissions: {
            type: "array",
            items: { type: "string" },
            example: ["polls:read", "questions:read", "results:read"]
          }
        }
      },
      RoleResponse: {
        type: "object",
        properties: {
          id: { type: "string", example: "92b75286-d8f8-4061-9995-e6e23ccdee94" },
          name: { type: "string", example: "Admin" },
          isDeleted: { type: "boolean", example: false }
        }
      },
      RoleDetailResponse: {
        type: "object",
        properties: {
          id: { type: "string", example: "92b75286-d8f8-4061-9995-e6e23ccdee94" },
          name: { type: "string", example: "Admin" },
          isDeleted: { type: "boolean", example: false },
          permissions: {
            type: "array",
            items: { type: "string" },
            example: ["polls:read", "polls:add", "polls:update", "polls:delete", "questions:read"]
          }
        }
      },
      CreateUserRequest: {
        type: "object",
        required: ["firstName", "lastName", "email", "password", "roles"],
        properties: {
          firstName: { type: "string", minLength: 3, maxLength: 100, example: "Sarah" },
          lastName: { type: "string", minLength: 3, maxLength: 100, example: "Connor" },
          email: { type: "string", format: "email", example: "sarah.connor@surveybasket.com" },
          password: { type: "string", format: "password", minLength: 8, example: "SecurePass123!" },
          roles: {
            type: "array",
            items: { type: "string" },
            example: ["Member"]
          }
        }
      },
      UpdateUserRequest: {
        type: "object",
        required: ["firstName", "lastName", "email", "roles"],
        properties: {
          firstName: { type: "string", minLength: 3, maxLength: 100, example: "Sarah" },
          lastName: { type: "string", minLength: 3, maxLength: 100, example: "Reese" },
          email: { type: "string", format: "email", example: "sarah.reese@surveybasket.com" },
          roles: {
            type: "array",
            items: { type: "string" },
            example: ["Member", "Admin"]
          }
        }
      },
      UserResponse: {
        type: "object",
        properties: {
          id: { type: "string", example: "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
          firstName: { type: "string", example: "Sarah" },
          lastName: { type: "string", example: "Connor" },
          email: { type: "string", format: "email", example: "sarah.connor@surveybasket.com" },
          isDisabled: { type: "boolean", example: false },
          roles: {
            type: "array",
            items: { type: "string" },
            example: ["Member"]
          }
        }
      },
      ProblemDetails: {
        type: "object",
        properties: {
          type: { type: "string", example: "https://tools.ietf.org/html/rfc9110#section-15.5.1" },
          title: { type: "string", example: "Bad Request" },
          status: { type: "integer", example: 400 },
          detail: { type: "string", example: "One or more validation errors occurred." },
          instance: { type: "string", example: "/api/polls" },
          errors: {
            type: "array",
            items: { type: "string" },
            example: ["Poll.DuplicateTitle", "Another poll with the same title already exists."]
          }
        }
      }
    }
  }
};

// 2. Build Postman Collection v2.1.0
function buildPostmanCollection() {
  const collection = {
    info: {
      name: "Survey System API",
      description: "Complete Postman collection for Survey System API with full realistic request bodies, token lifecycle management, and status-code assertions.",
      schema: "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    auth: {
      type: "bearer",
      bearer: [
        {
          key: "token",
          value: "{{bearer_token}}",
          type: "string"
        }
      ]
    },
    variable: [
      { key: "baseUrl", value: "https://localhost:7108", type: "string" },
      { key: "bearer_token", value: "", type: "string" },
      { key: "refresh_token", value: "", type: "string" },
      { key: "pollId", value: "1", type: "string" },
      { key: "questionId", value: "1", type: "string" },
      { key: "roleId", value: "92b75286-d8f8-4061-9995-e6e23ccdee94", type: "string" },
      { key: "userId", value: "3fa85f64-5717-4562-b3fc-2c963f66afa6", type: "string" }
    ],
    item: [
      // FOLDER 1: Auth
      {
        name: "Auth",
        description: "Authentication, Registration, Token Refresh, and Password Recovery endpoints.",
        item: [
          {
            name: "Register",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  email: "member@surveybasket.com",
                  firstName: "John",
                  lastName: "Doe",
                  password: "Password123!"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/register",
                host: ["{{baseUrl}}"],
                path: ["auth", "register"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Login",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  email: "admin@surveybasket.com",
                  password: "Password123!"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth",
                host: ["{{baseUrl}}"],
                path: ["auth"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});',
                    '',
                    'if (pm.response.code === 200) {',
                    '    var json = pm.response.json();',
                    '    pm.test("Response contains JWT token", function () {',
                    '        pm.expect(json.token).to.be.a("string");',
                    '    });',
                    '    if (json.token) {',
                    '        pm.collectionVariables.set("bearer_token", json.token);',
                    '        console.log("Updated bearer_token variable");',
                    '    }',
                    '    if (json.refreshToken) {',
                    '        pm.collectionVariables.set("refresh_token", json.refreshToken);',
                    '        console.log("Updated refresh_token variable");',
                    '    }',
                    '    if (json.id) {',
                    '        pm.collectionVariables.set("userId", json.id);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Refresh Token",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  token: "{{bearer_token}}",
                  refreshToken: "{{refresh_token}}"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/refresh",
                host: ["{{baseUrl}}"],
                path: ["auth", "refresh"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});',
                    'if (pm.response.code === 200) {',
                    '    var json = pm.response.json();',
                    '    if (json.token) {',
                    '        pm.collectionVariables.set("bearer_token", json.token);',
                    '    }',
                    '    if (json.refreshToken) {',
                    '        pm.collectionVariables.set("refresh_token", json.refreshToken);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Revoke Refresh Token",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  token: "{{bearer_token}}",
                  refreshToken: "{{refresh_token}}"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/revoke-refresh-token",
                host: ["{{baseUrl}}"],
                path: ["auth", "revoke-refresh-token"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Confirm Email",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  userId: "{{userId}}",
                  code: "123456"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/confirm-email",
                host: ["{{baseUrl}}"],
                path: ["auth", "confirm-email"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Resend Confirmation Email",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  email: "member@surveybasket.com"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/resend-confirmation-email",
                host: ["{{baseUrl}}"],
                path: ["auth", "resend-confirmation-email"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Forget Password",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  email: "member@surveybasket.com"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/forget-password",
                host: ["{{baseUrl}}"],
                path: ["auth", "forget-password"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Reset Password",
            request: {
              auth: { type: "noauth" },
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  email: "member@surveybasket.com",
                  code: "123456",
                  newPassword: "NewSecurePassword123!"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/auth/reset-password",
                host: ["{{baseUrl}}"],
                path: ["auth", "reset-password"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 2: Account / Me
      {
        name: "Account",
        description: "Authenticated user profile management.",
        item: [
          {
            name: "Get Current User Profile",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/me",
                host: ["{{baseUrl}}"],
                path: ["me"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});',
                    'pm.test("Profile returns valid email and names", function () {',
                    '    var json = pm.response.json();',
                    '    pm.expect(json.email).to.be.a("string");',
                    '    pm.expect(json.firstName).to.be.a("string");',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Update Current User Profile",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  firstName: "Mohamed",
                  lastName: "El Helaly"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/me/info",
                host: ["{{baseUrl}}"],
                path: ["me", "info"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Change Current User Password",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  currentPassword: "Password123!",
                  newPassword: "NewPassword123!"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/me/change-password",
                host: ["{{baseUrl}}"],
                path: ["me", "change-password"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 3: Polls
      {
        name: "Polls",
        description: "Poll lifecycle and publishing management.",
        item: [
          {
            name: "Get All Polls",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls",
                host: ["{{baseUrl}}"],
                path: ["api", "polls"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});',
                    'pm.test("Response is an array of polls", function () {',
                    '    var json = pm.response.json();',
                    '    pm.expect(json).to.be.an("array");',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Current Polls (Member)",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/current",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "current"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Poll By ID",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Create Poll",
            request: {
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  title: "Customer Satisfaction Survey 2026",
                  summary: "Annual customer satisfaction and experience survey.",
                  startsAt: "2026-10-05",
                  endsAt: "2026-11-05"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/polls",
                host: ["{{baseUrl}}"],
                path: ["api", "polls"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 201 Created", function () {',
                    '    pm.response.to.have.status(201);',
                    '});',
                    'if (pm.response.code === 201) {',
                    '    var json = pm.response.json();',
                    '    if (json.id) {',
                    '        pm.collectionVariables.set("pollId", json.id.toString());',
                    '        console.log("Updated pollId variable to: " + json.id);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Update Poll",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  title: "Updated Customer Satisfaction Survey 2026",
                  summary: "Updated survey regarding customer satisfaction and platform usability.",
                  startsAt: "2026-10-05",
                  endsAt: "2026-11-20"
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Toggle Publish Poll",
            request: {
              method: "PUT",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/togglePublish",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "togglePublish"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Delete Poll",
            request: {
              method: "DELETE",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 4: Questions
      {
        name: "Questions",
        description: "Question and answer choice management nested under polls.",
        item: [
          {
            name: "Get Questions for Poll",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/questions?pageNumber=1&pageSize=10&sortDirection=Asc",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "questions"],
                query: [
                  { key: "pageNumber", value: "1" },
                  { key: "pageSize", value: "10" },
                  { key: "sortDirection", value: "Asc" },
                  { key: "searchValue", value: "", disabled: true },
                  { key: "sortColumn", value: "", disabled: true }
                ]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Question By ID",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/questions/{{questionId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "questions", "{{questionId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Create Question",
            request: {
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  content: "How satisfied are you with our platform responsiveness?",
                  answers: [
                    "Very Satisfied",
                    "Satisfied",
                    "Neutral",
                    "Dissatisfied"
                  ]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/questions",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "questions"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 201 Created", function () {',
                    '    pm.response.to.have.status(201);',
                    '});',
                    'if (pm.response.code === 201) {',
                    '    var json = pm.response.json();',
                    '    if (json.id) {',
                    '        pm.collectionVariables.set("questionId", json.id.toString());',
                    '        console.log("Updated questionId variable to: " + json.id);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Update Question",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  content: "How satisfied are you with our overall customer service?",
                  answers: [
                    "Highly Satisfied",
                    "Satisfied",
                    "Neutral",
                    "Needs Improvement"
                  ]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/questions/{{questionId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "questions", "{{questionId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Toggle Question Status",
            request: {
              method: "PATCH",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/questions/{{questionId}}/toggleStatus",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "questions", "{{questionId}}", "toggleStatus"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 5: Votes
      {
        name: "Votes",
        description: "Voting submission and poll questions retrieval for members.",
        item: [
          {
            name: "Start Vote / Get Questions for Member",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/vote",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "vote"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Submit Vote",
            request: {
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  answers: [
                    { questionId: 1, answerId: 1 },
                    { questionId: 2, answerId: 3 }
                  ]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/polls/{{pollId}}/vote",
                host: ["{{baseUrl}}"],
                path: ["api", "polls", "{{pollId}}", "vote"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 201 Created", function () {',
                    '    pm.response.to.have.status(201);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 6: Results
      {
        name: "Results",
        description: "Aggregated voting statistics and raw results.",
        item: [
          {
            name: "Get Poll Votes Raw Data",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/results/row-data?pollId={{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "results", "row-data"],
                query: [{ key: "pollId", value: "{{pollId}}" }]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Votes Per Day",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/results/votes-per-day?pollId={{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "results", "votes-per-day"],
                query: [{ key: "pollId", value: "{{pollId}}" }]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Votes Per Question",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/results/votes-per-question?pollId={{pollId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "results", "votes-per-question"],
                query: [{ key: "pollId", value: "{{pollId}}" }]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 7: Roles
      {
        name: "Roles",
        description: "Role and permission administration.",
        item: [
          {
            name: "Get All Roles",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/roles?includeDisabled=false",
                host: ["{{baseUrl}}"],
                path: ["api", "roles"],
                query: [{ key: "includeDisabled", value: "false" }]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get Role By ID",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/roles/{{roleId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "roles", "{{roleId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Create Role",
            request: {
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  name: "SurveyManager",
                  permissions: [
                    "polls:read",
                    "polls:add",
                    "polls:update",
                    "questions:read",
                    "questions:add",
                    "questions:update",
                    "results:read"
                  ]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/roles",
                host: ["{{baseUrl}}"],
                path: ["api", "roles"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 201 Created", function () {',
                    '    pm.response.to.have.status(201);',
                    '});',
                    'if (pm.response.code === 201) {',
                    '    var json = pm.response.json();',
                    '    if (json.id) {',
                    '        pm.collectionVariables.set("roleId", json.id);',
                    '        console.log("Updated roleId variable to: " + json.id);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Update Role",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  name: "SeniorSurveyManager",
                  permissions: [
                    "polls:read",
                    "polls:add",
                    "polls:update",
                    "polls:delete",
                    "questions:read",
                    "questions:add",
                    "questions:update",
                    "results:read"
                  ]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/roles/{{roleId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "roles", "{{roleId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Toggle Role Status",
            request: {
              method: "PUT",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/roles/{{roleId}}/toggle-status",
                host: ["{{baseUrl}}"],
                path: ["api", "roles", "{{roleId}}", "toggle-status"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 8: Users
      {
        name: "Users",
        description: "User account management and authorization assignment.",
        item: [
          {
            name: "Get All Users",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/users",
                host: ["{{baseUrl}}"],
                path: ["api", "users"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Get User By ID",
            request: {
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/users/{{userId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "users", "{{userId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Create User",
            request: {
              method: "POST",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  firstName: "Sarah",
                  lastName: "Connor",
                  email: "sarah.connor@surveybasket.com",
                  password: "Password123!",
                  roles: ["Member"]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/users",
                host: ["{{baseUrl}}"],
                path: ["api", "users"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 201 Created", function () {',
                    '    pm.response.to.have.status(201);',
                    '});',
                    'if (pm.response.code === 201) {',
                    '    var json = pm.response.json();',
                    '    if (json.id) {',
                    '        pm.collectionVariables.set("userId", json.id);',
                    '        console.log("Updated userId variable to: " + json.id);',
                    '    }',
                    '}'
                  ]
                }
              }
            ]
          },
          {
            name: "Update User",
            request: {
              method: "PUT",
              header: [{ key: "Content-Type", value: "application/json" }],
              body: {
                mode: "raw",
                raw: JSON.stringify({
                  firstName: "Sarah",
                  lastName: "Reese",
                  email: "sarah.reese@surveybasket.com",
                  roles: ["Member", "Admin"]
                }, null, 2)
              },
              url: {
                raw: "{{baseUrl}}/api/users/{{userId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "users", "{{userId}}"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Toggle User Status",
            request: {
              method: "PUT",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/users/{{userId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "users", "{{userId}}", "toggle-status"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          },
          {
            name: "Unlock User",
            request: {
              method: "PUT",
              header: [],
              url: {
                raw: "{{baseUrl}}/api/users/{{userId}}",
                host: ["{{baseUrl}}"],
                path: ["api", "users", "{{userId}}", "unlock"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 204 No Content", function () {',
                    '    pm.response.to.have.status(204);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      },
      // FOLDER 9: Health
      {
        name: "Health",
        description: "API and dependency health check.",
        item: [
          {
            name: "Health Check",
            request: {
              auth: { type: "noauth" },
              method: "GET",
              header: [],
              url: {
                raw: "{{baseUrl}}/health",
                host: ["{{baseUrl}}"],
                path: ["health"]
              }
            },
            event: [
              {
                listen: "test",
                script: {
                  type: "text/javascript",
                  exec: [
                    'pm.test("Status code is 200 OK", function () {',
                    '    pm.response.to.have.status(200);',
                    '});'
                  ]
                }
              }
            ]
          }
        ]
      }
    ]
  };

  return collection;
}

// Convert object to YAML string (simplified converter for clean YAML output)
function toYaml(obj, indent = 0) {
  const pad = '  '.repeat(indent);
  if (obj === null || obj === undefined) return 'null\n';
  if (typeof obj === 'boolean' || typeof obj === 'number') return `${obj}\n`;
  if (typeof obj === 'string') {
    if (obj.includes('\n')) {
      const lines = obj.split('\n').map(l => `${pad}  ${l}`).join('\n');
      return `|\n${lines}\n`;
    }
    if (/[:#\[\]\{\},>&*?|<>=!%@`]/.test(obj) || obj === '' || /^\s|\s$/.test(obj)) {
      return `"${obj.replace(/"/g, '\\"')}"\n`;
    }
    return `${obj}\n`;
  }
  if (Array.isArray(obj)) {
    if (obj.length === 0) return '[]\n';
    let res = '\n';
    for (const item of obj) {
      if (typeof item === 'object' && item !== null) {
        const itemYaml = toYaml(item, indent + 1);
        const trimmed = itemYaml.trimStart();
        res += `${pad}- ${trimmed}`;
      } else {
        res += `${pad}- ${toYaml(item, 0)}`;
      }
    }
    return res;
  }
  if (typeof obj === 'object') {
    const keys = Object.keys(obj);
    if (keys.length === 0) return '{}\n';
    let res = indent === 0 ? '' : '\n';
    for (const key of keys) {
      const val = obj[key];
      const valYaml = toYaml(val, indent + 1);
      if (typeof val === 'object' && val !== null) {
        res += `${pad}${key}:${valYaml}`;
      } else {
        res += `${pad}${key}: ${valYaml}`;
      }
    }
    return res;
  }
  return String(obj) + '\n';
}

// Write files
const openApiJsonPath = path.join(__dirname, 'openapi.json');
const openApiYamlPath = path.join(__dirname, 'openapi.yaml');
const postmanPath = path.join(__dirname, 'Survey_System_API.postman_collection.json');

fs.writeFileSync(openApiJsonPath, JSON.stringify(openApiSpec, null, 2), 'utf8');
console.log(`Wrote OpenAPI JSON to: ${openApiJsonPath}`);

fs.writeFileSync(openApiYamlPath, toYaml(openApiSpec), 'utf8');
console.log(`Wrote OpenAPI YAML to: ${openApiYamlPath}`);

const postmanCollection = buildPostmanCollection();
fs.writeFileSync(postmanPath, JSON.stringify(postmanCollection, null, 2), 'utf8');
console.log(`Wrote Postman Collection to: ${postmanPath}`);
