using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _Repository;
        private readonly IMapper _Mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository _repository, IMapper _mapper, LinkGenerator linkGenerator)
        {
            _Repository = _repository;
            _Mapper = _mapper;
            this.linkGenerator = linkGenerator;
        }


        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talk = await _Repository.GetTalksByMonikerAsync(moniker, true);

                if (talk == null) return NotFound("There is no object by this moniker");

                return _Mapper.Map<TalkModel[]>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _Repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return NotFound("There is no object by this moniker");

                return _Mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }


        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _Repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                var talk = _Mapper.Map<Talk>(model);
                talk.Camp = camp;
                if (model.Speaker == null) return BadRequest("Speaker Id is required");
                var speaker = await _Repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");
                talk.Speaker = speaker;

                _Repository.Add(talk);

                if (await _Repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });

                    return Created(url, _Mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker,int id, TalkModel model)
        {
            try
            {
                var talk = await _Repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Couldn't find the talk");

                _Mapper.Map(model, talk);
                if (model.Speaker != null)
                {
                    var speaker = await _Repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _Repository.SaveChangesAsync())
                {
                    return _Mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TalkModel>> Delete(string moniker, int id)
        {
            var talk = await _Repository.GetTalkByMonikerAsync(moniker, id);
            if (talk == null) return NotFound("Failed to find the talk to delete");
            _Repository.Delete(talk);

            if (await _Repository.SaveChangesAsync())
            {
                return Ok();
            }
            else
            {
                return BadRequest("failed to delete talk");
            }
        }
    }
}