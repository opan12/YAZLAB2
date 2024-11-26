using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;

public class KategoriController : Controller
{
    private readonly ApplicationDbContext _context;

    public KategoriController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Kategori
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kategoriler = await _context.Kategoris.ToListAsync(); // Get all categories
        return View(kategoriler);
    }

    // GET: Kategori/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var kategori = await _context.Kategoris.FindAsync(id);
        if (kategori == null)
        {
            return NotFound();
        }
        return View(kategori);
    }

    // POST: Kategori/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("KategoriId,KategoriAdi,EtkinlikId")] Kategori kategori)
    {
        if (id != kategori.KategoriId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kategori);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Kategoris.Any(e => e.KategoriId == kategori.KategoriId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(kategori);
    }

    // GET: Kategori/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var kategori = await _context.Kategoris.FindAsync(id);
        if (kategori == null)
        {
            return NotFound();
        }

        return View(kategori);
    }

    // POST: Kategori/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var kategori = await _context.Kategoris.FindAsync(id);
        _context.Kategoris.Remove(kategori);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: KategoriEkle
    [HttpPost]
    public async Task<IActionResult> KategoriEkle([FromForm] Kategori kategori)
    {
        if (kategori != null && !string.IsNullOrEmpty(kategori.KategoriAdi))
        {
            _context.Kategoris.Add(kategori);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Redirect back to the list page
        }
        return BadRequest("Geçersiz kategori verisi.");
    }
}
