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
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _Repository;
        private readonly IMapper _Mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository _repository, IMapper _mapper, LinkGenerator linkGenerator)
        {
            _Repository = _repository;
            _Mapper = _mapper;
            this.linkGenerator = linkGenerator;
        }

        /*When ever there is async that operation becomes task and we have to add await to that task ans if that is the return type the return type of the method also changes
        
            */
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks = false)
        {
            try
            {
                var result = await _Repository.GetAllCampsAsync(includeTalks);

                /*We were using IactionResult as return type but to know if we want to give a specific return type this is how we do that by changing the return type to
                 Accurate model.*/
                return _Mapper.Map<CampModel[]>(result);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> GetCampsByMoniker(string moniker)
        {
            try
            {
                var result =  await _Repository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return _Mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "database error");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var result = await _Repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!result.Any()) return NotFound();

                return _Mapper.Map<CampModel[]>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {

                var exisitingMoniker = await _Repository.GetCampAsync(model.Moniker);
                if (exisitingMoniker != null)
                {
                    return BadRequest("Moniker already Exists ");
                }

                var location = linkGenerator.GetPathByAction("GetCampsByMoniker", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = _Mapper.Map<Camp>(model);

                _Repository.Add(camp);

                if (await _Repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", _Mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "database Error");
            }

            return BadRequest();
        }
       
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> put(string moniker, CampModel campModel)
        {
            try
            {
                var oldCamp = await _Repository.GetCampAsync(moniker);

                if (oldCamp == null) return BadRequest("This object doesnot exist wrong moniker");

                _Mapper.Map(campModel, oldCamp);

                if (await _Repository.SaveChangesAsync())
                {
                    return _Mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _Repository.GetCampAsync(moniker);

                if (oldCamp == null) return NotFound("We didnt found any object with this moniker");

                _Repository.Delete(oldCamp);

                if (await _Repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database error");
            }

            return BadRequest();
        }

    }
}