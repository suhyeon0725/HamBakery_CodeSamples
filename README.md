# HamBakery_CodeSamples

*햄빵 베이커리 프로젝트* 에서 발췌하여 정리한 코드 예시입니다.
전체 프로젝트가 아닌, 게임의 주요 기능별 구조와 동작 방식을 보여주기 위한 목적의 샘플 코드입니다.
각 스크립트는 Merge, UI, 캐릭터 AI, 오븐 시스템, 퀘스트 등 주요 기능을 독립적으로 구현한 코드입니다.

# 폴더 구조

Scripts/
├── UI/                        # 화면 전환 관련 스크립트
│   └── ActiveManager.cs       # 페이지 전환 제어

├── Character/                 # 캐릭터 AI 관련 스크립트
│   ├── CustomerManager.cs     # 손님 생성, 상태, 이동 제어
│   ├── CustomerPreScript.cs   # 손님 개별 행동 처리 (입장~퇴장)
│   └── HamsterPreScript.cs    # 서빙 햄스터 이동 및 음료 전달 로직

├── Merge/                     # 병합 및 베이커리 생성 시스템
│   └── MergeManager.cs        # 반죽 생성, 슬롯 저장, 병합 결과 생성

├── Kitchen/                   # 오븐, 요리 관련 로직
│   └── OvenPreScript.cs       # 베이커리 아이템 굽기 처리

├── Quest/                     # 퀘스트 진행 및 데이터 관리
│   ├── QuestManager.cs        # 퀘스트 진행 조건, 보상 처리
│   ├── QuestDatabase.cs       # 퀘스트 데이터베이스 (ScriptableObject)
│   └── QuestData.cs           # 퀘스트 정보 구조 정의 (ScriptableObject)


# 주요 스크립트 설명

## UI
 **ActiveManager.cs**: 게임 내 주요 화면(Main, Merge, Kitchen 등)의 활성화/비활성화를 관리합니다.  
                       각 화면 전환을 단일 메서드로 처리할 수 있도록 pageMap을 통해 Canvas를 매핑하여 제어합니다.
 
## Character
 **CustomerManager.cs**: 고객 프리팹을 일정 시간 간격으로 랜덤 위치에 생성합니다. 
                         Json 파일에서 고객별 TMI 데이터를 파싱하여 각 고객에게 설정합니다.
 **CustomerPreScript.cs**: 고객 개별 행동을 상태 기반으로 처리합니다.
                           입장(Walk/Enter) → 대기(Wait) → 선택(Select) → 주문(Order) → 착석 또는 포장(Seat/TakeOut) → 퇴장(Out) 흐름으로 구성되어 있으며,
                           NavMeshAgent와 Animator를 활용하여 이동 및 행동 애니메이션을 구현하였습니다.
 **HamsterPreScript.cs**: 서빙 역할의 햄스터가 음료를 가져와 손님에게 전달한 뒤 복귀하는 과정을 담당합니다.
                          서빙 상태에 따라 음료 기계 이동 → 음료 제작 → 손님 위치로 이동 → 음료 전달 → 제자리 복귀로 이루어져 있으며,
                          Coroutine을 통해 시간 지연과 보상 처리, 상태 전환을 자연스럽게 구성하였습니다.
 
## Merge
 **MergeManager.cs**: 슬롯의 해금/잠금 상태를 저장 및 불러오며, 슬롯에 반죽을 생성하고 데이터를 관리합니다.
                      머지 시 생성되는 베이커리는 확률 기반으로 레벨과 종류가 결정됩니다.
                      신규 레시피 등장 시 팝업 및 경험치 보상이 지급됩니다.

## Kitchen
 **OvenPreScript.cs**: 굽기 시작 시간과 종료 시간을 저장하며, 실제 시간 경과에 따라 아이템이 완성됩니다.
                       오븐 속도 업그레이드 수치에 따라 굽는 시간이 단축되며, 완료 시 UI로 알림이 표시됩니다.

## Quest
 **QuestManager.cs**: 현재 활성화된 퀘스트의 조건을 실시간으로 확인하고, 완료 시 보상을 지급합니다.
 **QuestData.cs**, **QuestDatabase.cs**: ScriptableObject를 기반으로 한 퀘스트 정의 구조입니다.
                                         각 퀘스트의 ID, 이름, 설명, 목표 수치, 보상 등 기본 정보를 담고 있으며,
                                         QuestManager에서 이를 불러와 동작하도록 구성되어 있습니다.


# 이 저장소에 포함된 코드는 햄빵 베이커리 프로젝트에서 기능별로 발췌한 예시 코드입니다.
# 전체 프로젝트의 세부 코드나 추가 구현이 궁금하시다면, 별도로 연락 주시면 감사하겠습니다.
