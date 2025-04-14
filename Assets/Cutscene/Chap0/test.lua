--- Test Cutscene

local tg1 = get("Target1")
local tg2 = get("Target2")

function playCutscene()
    log("Test Cutscene Start")
    
    wait(1.0)
    log("Test ActionManager")
    
    tg1:action():exec("TestVoidAction1")
    tg1:action():exec("TestVoidAction2")
    
    tg1:action():exec("TestIntAction1", 1)
    tg1:action():exec("TestIntAction2", 5)
    
    tg1:action():execAsync("TestIntActionAsync", 1000)
    log("Success")
    
    wait(2.0)
    
    tg1:camera():follow(0.03, true)
    tg2:camera():follow(0.03, true)
    
    log("Test Cutscene End")
end
