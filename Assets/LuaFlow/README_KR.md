<div align="center">

## LuaFlow

[![License: MIT](https://img.shields.io/badge/License-MIT-skyblue.svg?style=for-the-badge&logo=github)](LICENSE)
![GitHub Repo stars](https://img.shields.io/github/stars/snow0406/luaFlow?style=for-the-badge&logo=github&color=%23ef8d9d)

LuaFlow는 [UniTask](https://github.com/Cysharp/UniTask)와 [Lua-CSharp](https://github.com/kevthehermit/Lua-CSharp)를 기반으로 구축된 Lua 스크립트 기반 Unity 컷신 시스템입니다.

> 이 시스템은 [Lilium](https://hyuki.dev/project/lilium/) 프로젝트의 일부에서 가져왔습니다 !

</div>

---

## 개발 동기

LuaFlow의 개발 동기는 JSON 기반 컷신 관리의 불편함에서 시작되었습니다. 그냥 유니티의 인스펙터에서 구현한 컷씬 관리는 너무 귀찮았고 복잡했습니다, 그래서 다른걸 찾다가 Lua 스크립트, JSON을 보았고 루아를 이용해 컷씬 관리를 하면 간지가 날것 같아서 만들게 됬습니다 !!

### 아키텍처 배경

LuaFlow는 Scene Adaptive 방식을 사용하는 [Lilium](https://hyuki.dev/project/lilium/) 프로젝트에서 가져왔으며 이 접근 방식은 게임 요소를 독립적으로 로드하고 언로드할 수 있는 다양한 씬(예: 챕터별 씬, 플레이어 씬, 매니저 씬)으로 분리합니다.

이 방식은 다음과 같은 방식으로 LuaFlow의 코드 구조의 영향을 미칩니다:

- **인터페이스 기반 설계**: 시스템 컴포넌트는 구체적인 구현이 아닌 인터페이스를 통해 상호작용 합니다.

- **서비스 로케이터 패턴**: `LuaFlowServiceLocator`를 통해 다양한 매니저 인스턴스에 접근합니다.

- **중앙 집중식 엔티티 관리**: 서로 다른 씬에 서로 다른 게임 오브젝트가 포함되어 있으므로, `GameEntityManager`는 씬 경계를 넘어 오브젝트를 등록하고 접근하는 방법을 제공합니다.

## Lilium 프로젝트의 컷신 예제

LuaFlow를 이용한 [Lilium](https://hyuki.dev/project/lilium/) 프로젝트의 실제 컷신입니다:

```lua
--- Chap1-01 -> Chap1-02 컷신

-- 필요한 게임 오브젝트 참조 가져오기
local player = get("Player")
local camera1 = get("CameraMove_1")
local camera2 = get("CameraMove_2")

function playCutscene()
    -- 카메라를 0.5 속도로 카메라1 따라가기, 도착까지 대기
    camera1:camera():follow(0.5, true)

    -- 화면 페이드 아웃
    player:cinematic():fadeOut()
    camera2:camera():follow(0.03, true)

    -- 플레이어 이동 중지 함수 실행
    player:action():exec("PlayerMoveStop")

    -- 챕터 전환 이벤트 Invoke
    player:event():exec("MovePlayerToNextChapter")

    -- 플레이어 방향 오른쪽으로 뒤집기
    player:animation():flip(true)
    player:camera():follow(1, true)

    -- 플레이어 낙하 애니메이션 재생
    player:animation():play("FallDown")

    -- 화면 페이드 인
    player:cinematic():fadeIn()

    -- 3초 대기
    wait(3.0)

    -- 플레이어 일어나는 애니메이션 재생 및 완료까지 대기
    player:animation():play("GetUp", true)
    player:animation():play("Idle")

    -- 플레이어 컨트롤 다시 활성화 함수 실행
    player:action():exec("PlayerMoveStart")
end
```

## 시작하기

### 설치

LuaFlow는 UPM(Unity Package Manager)을 통해 설치할 수 있습니다:

1. 패키지 매니저 창 열기 (Window > Package Manager)
2. 좌측 상단의 "+" 버튼 클릭
3. "Add package from git URL..." 선택
4. `https://github.com/Snow0406/LuaFlow.git?path=Assets/LuaFlow` 입력
5. "Add" 클릭

패키지 의존성(Lua-CSharp 및 UniTask)은 package.json에 정의되어 있어 자동으로 설치됩니다.

> [Nuget Lua-CSharp v0.4.2](https://github.com/nuskey8/Lua-CSharp)은 dll 파일로 들어가있습니다.

### 프로젝트 구조

```
Assets/
├── Cutscene/
    └── Chap{X}/             # 챕터별로 구성된 Lua 스크립트 파일
        └── {cutscene_name}.lua

LuaFlow/
├── Runtime/
│   ├── Base/            # 기본 인터페이스 및 클래스
│   ├── Command/         # 커맨드 구현 (애니메이션, 카메라, ...등)
│   ├── Core/            # 핵심 기능 
│   ├── Entity/          # 게임 엔티티 래퍼
│   ├── Integration/     # 커스텀 액션 및 이벤트 시스템
│   ├── Interface/       # 시스템 컴포넌트 인터페이스
```

## 아키텍처

### 인터페이스 기반 설계

> `LuaFlow`는 인터페이스 기반 설계를 통해 높은 유연성과 확장성을 제공합니다.

1. **인터페이스 정의:**
```csharp
// Interface/ICameraManager.cs
public interface ICameraManager
{
    Vector3 PositionOffset { get; set; }
    Transform transform { get; }
    void ChangeCameraTarget(Transform target, float followSpeed = 0.1f);
}
```

2. **구현 클래스:**
```csharp
// CameraManager.cs
public class CameraManager : MonoBehaviour, ICameraManager
{
    public static CameraManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LuaFlowServiceLocator.Register<ICameraManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // ICameraManager 인터페이스 구현
    // ...
}
```

이 방식을 사용하면 다른 부분이 구체적인 클래스가 아닌 인터페이스에 의존하게 되어, 쉽게 변경하거나 대체할 수 있습니다.

### 서비스 로케이터 패턴

> `LuaFlowServiceLocator`는 다양한 매니저 인스턴스에 접근할 수 있는 중앙 레지스트리를 제공합니다. <br/>
> 이를 통해 커맨드 클래스와 매니저 간의 결합도를 낮출 수 있습니다.

1. **매니저 등록:**

```csharp
// 매니저 클래스의 Awake 메서드에서
LuaFlowServiceLocator.Register<ICameraManager>(this);

// 매니저 소멸 시 등록 해제
private void OnDestroy()
{
LuaFlowServiceLocator.Unregister<ICameraManager>();
}
```

2. **매니저 사용:**

```csharp
// 어디서든 등록된 매니저 인스턴스 접근
ICameraManager cameraManager = LuaFlowServiceLocator.Get<ICameraManager>();
cameraManager.ChangeCameraTarget(targetTransform, 0.5f);
```

## 사용법
### 게임 엔티티 관리
> `GameEntityManager`는 서로 다른 씬에서 게임 오브젝트를 등록, 접근 및 관리하는 중앙 집중식 방법을 제공합니다. <br/>
> 직접 구현해야 합니다. [예시]()

```csharp
// 게임 오브젝트 등록 (일반적으로 Awake 또는 Start에서)
GameEntityManager.RegisterGameObject("Player", playerGameObject);

// 등록된 게임 오브젝트 가져오기 (어디서든 호출 가능)
GameObject player = GameEntityManager.Instance.GetGameObject("Player");

// 더 이상 필요 없을 때 게임 오브젝트 등록 해제
GameEntityManager.UnregisterGameObject("Player");

// 모든 등록된 오브젝트 지우기 (예: 씬 변경 시)
GameEntityManager.Instance.RemoveAllEntities();
```

Lua 스크립트에서는 `get` 함수를 사용하여 등록된 게임 오브젝트에 접근할 수 있습니다:

```lua
-- Lua에서 등록된 게임 오브젝트 가져오기
local player = get("Player")
```

### 커스텀 이벤트 시스템

> `LuaCustomEventManager`는 C# 스크립트와 Lua 스크립트 간의 이벤트를 가능하게 하는 이벤트 시스템을 제공합니다.

**C#에서:**

```csharp
// 매개변수 없는 이벤트 구독
LuaCustomEventManager.Subscribe("PlayerDied", () => {
    Debug.Log("Player died event received!");
});

// 매개변수가 있는 이벤트 구독
LuaCustomEventManager.Subscribe<int>("ScoreChanged", (score) => {
    Debug.Log($"Score changed to: {score}");
});

// 이벤트 구독 해제
LuaCustomEventManager.Unsubscribe("PlayerDied", myCallback);
LuaCustomEventManager.Unsubscribe<int>("ScoreChanged", myScoreCallback);
```

**Lua에서:**

```lua
-- 매개변수 없는 이벤트 발행
myObject:event():exec("PlayerDied")

-- 매개변수가 있는 이벤트 발행
myObject:event():execP("ScoreChanged", 100)
```

### 커스텀 함수 시스템

> `LuaCustomActionManager`를 사용하면 Lua 스크립트에서 호출할 수 있는 C# 함수를 등록하는 시스템을 제공합니다.

**C#에서:**

```csharp
// 매개변수 없는 간단한 함수 등록
LuaCustomActionManager.RegisterFunction("ShowGameOver", () => {
    gameOverPanel.SetActive(true);
});

// 매개변수가 있는 함수 등록
LuaCustomActionManager.RegisterFunction("UpdateHealth", (int health) => {
    playerHealth.SetHealth(health);
});

private void UpdateHealth(int health) 
{
    playerHealth.SetHealth(health);
}

LuaCustomActionManager.RegisterFunction<int>("UpdateHealth", UpdateHealth);

// 비동기 함수 등록 (UniTask 사용)
LuaCustomActionManager.RegisterAsyncFunction("FadeToBlack", async () => {
    await fadeScreen.FadeToBlackAsync(2.0f);
});

// 더 이상 필요 없을 때 함수 등록 해제
LuaCustomActionManager.UnRegisterFunction("ShowGameOver");
```

**Lua에서:**

```lua
-- 매개변수 없는 등록된 함수 실행
myObject:action():exec("ShowGameOver")

-- 매개변수가 있는 등록된 함수 실행
myObject:action():exec("UpdateHealth", 50)

-- 비동기 함수 실행 (두 번째 매개변수가 true면 완료될 때까지 대기)
myObject:action():execAsync("FadeToBlack", true)
```

### 컷신 만들기

1. `Assets/Cutscene/Chap{X}/` 디렉토리에 새 Lua 스크립트 생성
2. 다음 템플릿으로 시작:

```lua
--- 테스트 컷신

-- GameEntityManager에 등록된 게임 오브젝트 참조 가져오기
local tg1 = get("Target1")
local tg2 = get("Target2")

-- 메인 컷신 함수
function playCutscene()
    log("테스트 컷신 시작")

    -- 2초 대기
    wait(2.0)

    -- 카메라가 부드럽게 Target1을 따라가도록 전환
    tg1:camera():follow(0.03, true)

    -- 카메라가 Target2를 따라가도록 전환
    tg2:camera():follow(0.03, true)

    log("테스트 컷신 종료")
end
```

### 컷신 트리거하기

`CutsceneTrigger` 컴포넌트를 사용하여 컷신을 시작할 수 있습니다:

1. 씬에 빈 GameObject 생성
2. `CutsceneTrigger` 스크립트 추가
3. Chapter와 Cutscene Name 필드 설정
4. 플레이어가 트리거 영역에 들어가면 컷신이 재생됩니다

## 시스템 확장하기
### 커스텀 커맨드 추가

> LuaFlow는 확장을 편하게 할수 있게 설계되었습니다.

새 커맨드 클래스를 만들어 LuaFlow를 확장할 수 있습니다:

1. `BaseLuaCommand`를 상속하는 새 클래스 생성
2. `[LuaObject]` 어트리뷰트 적용
3. `[LuaMember]` 어트리뷰트로 메서드 구현
4. 적절한 엔티티 래퍼에 클래스 등록

예제:

```csharp
[LuaObject]
public partial class LuaDialogueCommand : BaseLuaCommand
{
    public LuaDialogueCommand(GameObject targetObject) : base(targetObject)
    {
    }

    [LuaMember("say")]
    public void Say(string text)
    {
        // 구현
    }
}
```

### 커스텀 매니저 생성

1. **새 인터페이스 정의:**
```csharp
// Interface/IDialogueManager.cs
public interface IDialogueManager
{
    void ShowDialogue(string text);
    void HideDialogue();
    bool IsDialogueActive { get; }
}
```

2. **인터페이스 구현:**
```csharp
// DialogueManager.cs
public class DialogueManager : MonoBehaviour, IDialogueManager
{
    public static DialogueManager Instance { get; private set; }
    
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;
    
    public bool IsDialogueActive => dialoguePanel.activeSelf;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LuaFlowServiceLocator.Register<IDialogueManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowDialogue(string text)
    {
        // ...
    }
    
    public void HideDialogue()
    {
        // ...
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            LuaFlowServiceLocator.Unregister<IDialogueManager>();
        }
    }
}
```

## 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다 - 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

---

Made with ♥ by [hy](https://hyuki.dev)
