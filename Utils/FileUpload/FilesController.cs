    public class FilesController: BistoonControllerBase
    {
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("UploadedFiles");
                var pathToSave = $"c:\\{folderName}";

                if (file.Length > 0)
                {
                    if ((file.Length / 1024 / 1024) > 1)
                    {
                        Logger.Error($"File size limit exeeded");
                        return StatusCode(500, "File size limit exeeded");
                    }
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { fileName });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in upload file", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public IActionResult Get(string id)
        {
            try
            {
                var folderName = Path.Combine("UploadedFiles");
                var pathToGet = $"c:\\{folderName}";

                var filePath = Path.Combine(pathToGet, id);
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                var extension = Path.GetExtension(id).ToLower();
                return File(fileBytes, MimeTypeMap.GetMimeType(extension), id);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in getting file", ex);
                return StatusCode(500);
            }
        }

