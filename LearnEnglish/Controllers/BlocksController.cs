using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnEnglish.Data;
using LearnEnglish.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace LearnEnglish.Controllers
{
    public class BlocksController : Controller
    {
        private readonly EnglishDbContext _context;

        public BlocksController(EnglishDbContext context) => _context = context;

        public async Task<IActionResult> Create(int id)
        {
            ViewData["ArticleId"] = id;
            var blocks = await _context.Blocks.ToListAsync();
            int currentOrder = 1;
            var blocksWithId = blocks.Where(b => b.ArticleId == id);

            if (blocksWithId.Any())
            {
                currentOrder = blocksWithId.Max(b => b.Order) + 1;
            }

            ViewData["CurrentOrder"] = currentOrder;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int articleId, [FromForm] BlockType type, [FromForm] string content, [FromForm] int order)
        {
            if (ModelState.IsValid)
            {
                var block = new Block
                {
                    ArticleId = articleId,
                    Type = type,
                    Content = content,
                    Order = order
                };

                _context.Add(block);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AddBlock", "Articles", new {id = articleId});
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var block = await _context.Blocks.FindAsync(id);

            if (block == null)
                return NotFound();

            var article = await _context.Articles.FindAsync(block.ArticleId);

            //ViewData["ArticleId"] = new SelectList(_context.Articles, "Id", "Id", block.ArticleId);
            ViewData["ArticleId"] = article.Id;
            ViewData["Order"] = block.Order;

            return View(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int id, [FromForm] BlockType type, [FromForm] string content)
        {
            var block = await _context.Blocks.FindAsync(id);
            block.Type = type;
            block.Content = content;

            if (id != block.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(block);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlockExists(block.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("AddBlock", "Articles", new { id = block.ArticleId });
            }

            ViewData["ArticleId"] = new SelectList(_context.Articles, "Id", "Id", block.ArticleId);
            return View(block);
        }

        public async Task<IActionResult> RaiseBlockUp(int id)
        {
            var movableBlock = await _context.Blocks.FindAsync(id);
            await MoveBlock(movableBlock, -1);
            return RedirectToAction("AddBlock", "Articles", new { id = movableBlock.ArticleId });
        }

        public async Task<IActionResult> LowerBlockDown(int id)
        {
            var movableBlock = await _context.Blocks.FindAsync(id);
            await MoveBlock(movableBlock, 1);
            return RedirectToAction("AddBlock", "Articles", new { id = movableBlock.ArticleId });
        }

       
        public async Task<IActionResult> Delete(int id)
        {
            List<Block> blocks = await _context.Blocks.ToListAsync();
            Block deletedBlock = blocks.FirstOrDefault(b => b.Id == id);

            int articleId = deletedBlock.ArticleId;

            var filteredBlocks = blocks.Where(b => b.Order > deletedBlock.Order);

            if (deletedBlock != null)
                _context.Blocks.Remove(deletedBlock);

            foreach(var block in filteredBlocks)
                block.Order--;

            await _context.SaveChangesAsync();

            return RedirectToAction("AddBlock", "Articles", new { id = articleId });
        }

        private bool BlockExists(int id) => _context.Blocks.Any(e => e.Id == id);

        private async Task MoveBlock(Block movableBlock, int newOrderOffset)
        {
            if (movableBlock == null)
                return;

            var blocks = await _context.Blocks.ToListAsync();
            int oldOrder = movableBlock.Order;
            int newOrder = oldOrder + newOrderOffset;
            int articleId = movableBlock.ArticleId;

            var slidingBlock = blocks.FirstOrDefault(b => b.ArticleId == movableBlock.ArticleId && b.Order == newOrder);

            if (slidingBlock != null)
            {
                slidingBlock.Order = oldOrder;
                movableBlock.Order = newOrder;
                await _context.SaveChangesAsync();
            }
        }
    }
}
