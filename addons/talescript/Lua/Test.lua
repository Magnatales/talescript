move("TownGuard", 100, 100)
say_once("Welcome to this City, traveler.") -- This will only be said once.
say("What do you want?")

local index = choice(
    "Just exploring.",
    "Move, peasant."
)

local follow_up = {
    [1] = "Then explore quietly.",
    [2] = "Try that again and you'll eat dirt."
}

local amount_said = get_key(follow_up[index])

if index == 2 and amount_said == 2 then
    say("I warned you, now you will pay the price!")
    make_aggressive("TownGuard")
    return
end

say(follow_up[index])



--local was_called = say_once("Welcome to our town!")
--
--if was_called then
--    say("What the fuck do you want?")
--else
--    say("You are going to be very happy here!")
--end
--
say_once("Hello I'm a guard! Who are you?")
local index = choice("key_why_do_you_ask", "test2", "test3")
local after_choice = {
    [1] = "Good choice",
    [2] = "Decent choice",
    [3] = "Perfect choice"
}
local follow_up = after_choice[index]
say(follow_up)
-- local was_shown = once("12345")
-- if was_shown then
--     say("Once region second time start")
--     say("Once region second time end")
-- else
--     say("Once region start")
--     say("Once region end")
-- end

-- local was_shown2 = once("12345")
-- if was_shown2 then
--     say("Once region second time start")
--     say("Once region second time end")
-- else
--     say("Once region start")
--     say("Once region end")
-- end




