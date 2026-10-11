using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_label_service.Models;

[Route("api/[controller]")]
[ApiController]
public class TracksController : ControllerBase
{
    private readonly dotnet_label_serviceContext _context;
    public TracksController(dotnet_label_serviceContext context)
    {
        _context = context;
    }

    // GET: api/Track
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Track>>> GetTrack()
    {
        return await _context.Track.ToListAsync();
    }

    // GET: api/Track/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Track>> GetTrack(System.Guid id)
    {
        var track = await _context.Track.FindAsync(id);

        if (track == null)
        {
            return NotFound();
        }

        return track;
    }

    // PUT: api/Track/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTrack(Guid id, CreateTrackRequest trackRequest)
    {
        Track? track = await _context.Track.FindAsync(id);

        if (track == null)
        {
            return NotFound();
        }

        track.Title = trackRequest.Title;
        track.Artist = trackRequest.Artist;
        track.Label = trackRequest.Label;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TrackExists(id))
            {
                return NotFound();
            }
            else
            {
                return Conflict();
            }
        }

        return NoContent();
    }

    // POST: api/Track
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Track>> PostTrack(CreateTrackRequest trackRequest)
    {
        var track = new Track
        {
            Title = trackRequest.Title,
            Artist = trackRequest.Artist,
            Label = trackRequest.Label,
        };

        _context.Track.Add(track);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrack), new { id = track.Id }, track);
    }

    // DELETE: api/Track/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrack(Guid id)
    {
        var track = await _context.Track.FindAsync(id);
        if (track == null)
        {
            return NotFound();
        }

        _context.Track.Remove(track);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TrackExists(Guid id)
    {
        return _context.Track.Any(e => e.Id == id);
    }
}
