# MyPet - Desktop Pet Utility 🐾

가장 직관적이고 아름다운 데스크탑 펫 애플리케이션입니다.

## 🌟 프로젝트 현재 상태 및 진행 현황 (2026-02-21 기준)

이 문서는 다음 개발 세션(다른 환경 포함)에서 인공지능 어시스턴트가 프로젝트의 맥락을 완벽하게 파악할 수 있도록 작성되었습니다.

### **1. 핵심 아키텍처 (Completed)**
*   **다중 펫 지원 (Multi-Pet Architecture)**: 모든 펫 관련 컴포넌트(`PetStateMachine`, `PetGrowthController`, `PetVisualManager` 등)에서 싱글톤을 제거하고 컴포넌트 기반 상호 참조 구조로 리팩토링 완료. 이제 여러 마리를 복사해도 독립적으로 작동합니다.
*   **독립 세이브 시스템**: `pet_data_ID.json` 형식으로 각 펫 인스턴스가 고유한 성장 데이터와 상태를 저장합니다.
*   **상호작용 소유권 (Interaction Ownership)**: 겹쳐 있는 펫 중 클릭한 특정 펫만 레이캐스트를 통해 정확히 식별하고 방사형 메뉴를 소유합니다.

### **2. 구현 완료 기능 (Working)**
*   **성장 및 상태 머신**: 0~5단계 성장 로직 및 Idle/Move/Interact/Struggling 상태 전환.
*   **허기(Hunger) 시스템**: 시간에 따른 실시간/오프라인 허기 감소 및 속도 페널티.
*   **방사형 메뉴 (Radial Menu)**: 글래스모피즘(Glassmorphism) 스타일의 UI 및 펫별 독립 대상 설정.
*   **유연한 비주얼 시스템**: 2D 스프라이트 시트 및 3D 모델 프레임워크 동시 지원.

### **3. 미해결 과제 (Incomplete - Urgent! 🛠️)**
*   **Phase 7: 윈도우 시스템 통합 (NOT WORKING)**
    *   `Win32Bridge.cs`를 통한 창 핸들(HWND) 획득은 성공함(Log 확인).
    *   **원인 규명**: `Player.log` 분석 결과 `[Tray]` 관련 로그가 전혀 없음 -> `TrayIconManager`와 `HotkeyListener` 컴포넌트가 **씬(Scene)의 어떤 오브젝트에도 붙어있지 않거나 비활성화 상태**인 것이 확실함.
    *   유니티 에디터에서는 안전을 위해 비활성화되어 있음.

---

## �️ 필수 설정 체크리스트 (새 환경에서 확인!)

새로운 환경에서 프로젝트를 열었을 때, **Phase 7(트레이, 투명화)**을 작동시키려면 반드시 다음을 확인하세요:

1.  **GlobalSystem 오브젝트 (Hierarchy)**: `IngameScene` 내에서 이미 `Win32Bridge`가 붙어있는 오브젝트를 찾으세요. (아마 `GlobalSystem` 혹은 `Win32Bridge`라는 이름일 것입니다.)
    *   그 오브젝트에 `TrayIconManager.cs`와 `HotkeyListener.cs` 스크립트를 드래그 앤 드롭으로 추가하세요.
    *   **이제 한 오브젝트에 3개(`Win32Bridge`, `TrayIconManager`, `HotkeyListener`)가 모두 있어야 합니다.**
2.  **Pet (Prefab) 설정**: 복사된 펫들이 제각각 놀게 하려면:
    *   **이름 바꿔주기 (추천)**: 하이어라키에서 펫 오브젝트의 이름(예: `Dog_Red`, `Dog_Blue`)만 다르게 지어주면, 제가 심어놓은 **'Smart ID'** 시스템이 알아서 별도의 세이브 파일을 생성합니다.
    *   (직접 설정 시): 각 펫의 `PetGrowthController` 컴포넌트 내 `Pet ID` 값을 고유하게 지정해도 됩니다.
3.  **빌드 옵션**: `File > Build Settings`에서 `Standalone Windows` 플랫폼인지, `IngameScene`이 0번으로 등록되어 있는지 확인.

---

## 🚀 다음 개발 가이드 (Next Assistant Start Here)

1.  **로그 분석 결과**: 현재 HWND 핸들 획득(예: `853772`)까지는 성공했으나, 트레이 관리 클래스들이 아예 실행(`Start`)되지 않고 있음. 씬에 매니저 오브젝트를 배치하는 것이 급선무.
2.  **레이블 보강**: `isClickThrough` 상태가 변할 때 윈도우 알림(Balloon Tip)이 뜨도록 `TrayIconManager.ShowNotification` 연동 필요.

---

## 🛠️ 개발 환경 주의사항 (Handover)
*   **Git Clone 후**: 모든 컴포넌트 설정값은 `.meta` 파일에 저장되어 있으므로 그대로 유지됩니다.
*   **다중 펫 테스트**: 펫 프리팹을 **`Ctrl+D`**로 복사한 뒤, 인스펙터의 `PetGrowthController`에서 **`Pet ID`**를 고유하게(0, 1, 2...) 지정해 주세요.
*   **빌드 설정**: `Standalone Windows` 필수.

---

## 🚪 앱 종료 방법 (Emergency Exit)

현재 트레이 아이콘이 표시되지 않는 문제가 있을 경우, 다음 방법으로 앱을 안전하게 종료할 수 있습니다:

1.  **라디얼 메뉴 (추천)**: 펫을 길게 클릭하여 라디얼 메뉴를 띄운 뒤, **하단(6시 방향)**으로 마우스를 가져가면 "종료" 기능이 작동합니다.
2.  **작업 관리자 (강제)**: `Ctrl + Shift + Esc`를 눌러 작업 관리자를 연 뒤, `MyPet.exe`를 찾아 '작업 끝내기'를 하세요.
