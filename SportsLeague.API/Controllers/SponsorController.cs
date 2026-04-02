using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using SportsLeague.API.DTOs.Request;

using SportsLeague.API.DTOs.Response;

using SportsLeague.Domain.Entities;

using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Services;


namespace SportsLeague.API.Controllers;


[ApiController]

[Route("api/[controller]")]


    public class SponsorController : ControllerBase
    {
        private readonly ISponsorService _sponsorService;

        private readonly IMapper _mapper;

        private readonly ILogger<SponsorController> _logger;

    public SponsorController(

            ISponsorService sponsorService,

            IMapper mapper,

            ILogger<SponsorController> logger)

    {

            _sponsorService = sponsorService;

            _mapper = mapper;

            _logger = logger;

    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sponsors = await _sponsorService.GetAllAsync();

        var sponsorDtos = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);

        return Ok(sponsorDtos);
    }
    [HttpGet("{id}")]

    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)

    {

        var sponsor = await _sponsorService.GetByIdAsync(id);


        if (sponsor == null)

            return NotFound(new { message = $"Sponsor con ID {id} no encontrado" });


        var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);

        return Ok(sponsorDto);

    }

    //POST
    [HttpPost]

    public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)

    {
                try

        {

                    var sponsor = _mapper.Map<Sponsor>(dto);

                    var createdSponsor = await _sponsorService.CreateAsync(sponsor);


                    var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);


                    return CreatedAtAction(

                    nameof(GetById),

                    new { id = responseDto.Id },

                    responseDto);

                }

                catch (KeyNotFoundException ex)

                    {

                return NotFound(new { message = ex.Message });

                    }

                catch (InvalidOperationException ex)

                   {

                 return Conflict(new { message = ex.Message });

        }



    }
    //PUT
    [HttpPut("{id}")]

    public async Task<ActionResult> Update(int id, SponsorRequestDTO dto)

    {

        try

        {

            var sponsor = _mapper.Map<Sponsor>(dto);

            await _sponsorService.UpdateAsync(id, sponsor);

            return NoContent();

        }

        catch (KeyNotFoundException ex)

        {

            return NotFound(new { message = ex.Message });

        }

        catch (InvalidOperationException ex)

        {

            return Conflict(new { message = ex.Message });

        }

    }

    [HttpDelete("{id}")]

    public async Task<ActionResult> Delete(int id)

    {

        try

        {

            await _sponsorService.DeleteAsync(id);

            return NoContent();

        }

        catch (KeyNotFoundException ex)

        {

            return NotFound(new { message = ex.Message });

        }

    }
    //Vincular sponsor a torneo
    [HttpPost("{id}/tournaments")]
    public async Task<ActionResult<TournamentSponsorResponseDTO>> LinkToTournament(
        int id,
        TournamentSponsorRequestDTO dto)
    {
        try
        {
            var relation = await _sponsorService
                .LinkToTournamentAsync(id, dto.TournamentId, dto.ContractAmount);

            var response = _mapper.Map<TournamentSponsorResponseDTO>(relation);

            return CreatedAtAction(nameof(GetTournamentsBySponsor), new { id }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    //Obtener torneos de un sponsor
    [HttpGet("{id}/tournaments")]
    public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetTournamentsBySponsor(int id)
    {
        try
        {
            var relations = await _sponsorService.GetTournamentsBySponsorAsync(id);

            var response = _mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(relations);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    //Desvincular sponsor de torneo
    [HttpDelete("{sponsorId}/tournaments/{tournamentId}")]
    public async Task<ActionResult> UnlinkFromTournament(int sponsorId, int tournamentId)
    {
        try
        {
            await _sponsorService.UnlinkFromTournamentAsync(sponsorId, tournamentId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }


}



