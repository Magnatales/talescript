---@meta

---@param text string
--- Prints a message to the console.
function print(text) end

---@param ... any
--- Pretty prints any object to the console.
function pprint(...) end

---@async
---@param key string
--- Says something.
function say(key) end

---@param key string
function make_aggressive(key) end

---@async
---@param ... string Keys
---@return number
function choice(...) end

---@async
---@param what string
---@return boolean was_shown
--- Says once. Returns if it's the first time.
function say_once(what) end

---@enum MOOD
MOOD = {
    neutral = "neutral",
    happy = "happy",
    sad = "sad",
    angry = "angry",
    surprised = "surprised",
    confused = "confused",
}

---@param name string
---@param mood? MOOD
--- Displays the left speaker with an optional mood.
function speaker_left(name, mood) end

---@param name string
---@param mood? string
--- Displays the right speaker with an optional mood.
function speaker_right(name, mood) end

--- Closes the left speaker.
function close_speaker_left() end

--- Closes the right speaker.
function close_speaker_right() end

--- Closes the dialog box.
function close_dialog() end

---@param key string
---@return boolean was_shown
function once(key) end

---@param path string
--- Jumps to another .lua file.
function detour(path) end

---@async
---@param seconds number
--- Waits for a number of seconds.
function wait(seconds) end

---@async
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
function get_key(key) end

---@param question string
---@param options string[]
---@return number
--- Presents a choice and returns the selected index (1-based).
function choice(question, options) end