﻿using ApplicationLayer.Commands.JobPostCommands;
using ApplicationLayer.Dtos;
using ApplicationLayer.Extensions; 
using ApplicationLayer.Queries.JobPostQueries; 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public JobPostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllAsync()
        {
            var jobPosts = await _mediator.Send(new JobPostGetAllQuery());
            return jobPosts.Any() ? Ok(jobPosts.Select(x => x.MapJopPostDomainToDto()).OrderByDescending(x=>x.PostedDate)) 
                                  : NotFound("No job post was found");
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var jobPost = await _mediator.Send(new JobPostByIdQuery { Id = id });
            return jobPost is not null ? Ok(jobPost.MapJopPostDomainToDto()) : NotFound("No job post was found");
        }

        [HttpGet("companyposts/{id:guid}")]
        public async Task<ActionResult> GetCompanyJobPostsAsync([FromRoute] Guid id)
        {
            var jobPosts = await _mediator.Send(new GetCompanyJobPostsQuery { Id = id });
            return jobPosts is not null ? Ok(jobPosts.Select(x => x.MapJopPostDomainToDto()).OrderByDescending(x => x.PostedDate)) 
                                       : NotFound("No job post was found for this company");
        }

        [HttpGet("search")]
        public async Task<ActionResult> GetJobPostsBySearchAsync([FromQuery] string description, [FromQuery] string location, [FromQuery] decimal salaryTo, [FromQuery] Guid jobTypeId)
        {
            var jobPosts = await _mediator.Send(new GetJobPostsBySearchQuery { Description = description, Location = location, SalaryTo = salaryTo, JobTypeId = jobTypeId });
            return jobPosts is not null ? Ok(jobPosts.Select(x => x.MapJopPostDomainToDto()).OrderByDescending(x => x.PostedDate)) 
                                       : NotFound("No job post matching your search");
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] CreateJobPostDto data)
        {
            var newJobPost = await _mediator.Send(new JobPostCreateCommand { Entity = data.MapCreateJopPostDtoToDomain() });
            return newJobPost.SuccessOrNot ? Ok(newJobPost.Message) : BadRequest(newJobPost.Message);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateJobPostDto data)
        {
            var jobPost = await _mediator.Send(new JobPostByIdQuery { Id = id });
            if (jobPost is null) return NotFound("No job post was found");
            var updatedJobPost = await _mediator.Send(new JobPostUpdateCommand { Entity = data.MapUpdateJopPostDtoToDomain(jobPost) });
            return updatedJobPost.SuccessOrNot ? Ok(updatedJobPost.Message) : BadRequest(updatedJobPost.Message);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var deletedJobPost = await _mediator.Send(new JobPostDeleteCommand { Id = id });
            return deletedJobPost.SuccessOrNot ? Ok(deletedJobPost.Message) : BadRequest(deletedJobPost.Message);
        }
    }
}
