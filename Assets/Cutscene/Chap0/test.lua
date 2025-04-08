--- Test Cutscene

local tg1 = get("Target1")
local tg2 = get("Target2")

function playCutscene()
    log("Test Cutscene Start")
    
    wait(2.0)
    
    tg1:camera():follow(0.03, true)
    tg2:camera():follow(0.03, true)
    
    log("Test Cutscene End")
end
