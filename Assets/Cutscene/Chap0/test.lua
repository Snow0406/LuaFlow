--- Test Cutscene

local tg1 = get("Target1")
local tg2 = get("Target2")
local latte = get("StrawBerryLatte")

function playCutscene()
    log("Test Cutscene Start")
    
    wait(1.0)
    
    tg1:action():execAsync("TestIntActionAsync", 1000)
    
    wait(2.0)
    
    tg1:camera():follow(0.03, true)
    tg2:camera():follow(0.03, true)

    latte:move():speed(6.0)
    latte:move():to(19, 3.5)

    latte:move():toSync(19, -3.5, 2)
    latte:camera():follow(0.1, false)
    latte:action():exec("TestVoidAction1")
    
    log("Test Cutscene End")
end
