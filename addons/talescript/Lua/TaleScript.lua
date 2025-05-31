---@meta

---@param message string
--- Prints a message to the console.
function print(message) end

---@param ... any
--- Pretty prints any object to the console.
function pprint(...) end

---@param dialog string
---@return boolean was_shown
--- Only runs a dialog once. Returns if it's the first time.
function once(dialog) end

---@param path string
--- Jumps to another .lua file.
function detour(path) end

---@param seconds number
--- Waits for a number of seconds.
function wait(seconds) end

---@param actor string
---@param x number
---@param y number
--- Moves a character to the specified position.
function move(actor, x, y) end

---@param actor string
---@param expression string
--- Changes the character's facial expression or emotion.
function emotion(actor, expression) end

---@param track string
---@return number duration
--- Plays background music. Returns the duration of the track in seconds.
function music(track) end

---@param sfx string
---@return number duration
--- Plays a sound effect. Returns the duration of the sound effect in seconds.
function sound(sfx) end

---@param name string
---@param mood? string
--- Displays a character name and optional mood.
function label(name, mood) end

---@param key string
---@param value any
--- Sets a global variable.
function set(key, value) end

---@param key string
---@return any
--- Gets a global variable.
function get(key) end

---@param question string
---@param options string[]
---@return number
--- Presents a choice and returns the selected index (1-based).
function choice(question, options) end