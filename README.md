# 🎮 ActionFit Code Test - Unity Client Developer

---

## ✅ 목표

- **제출 일시**: 2025년 5월 14일 13:00까지  
- **제출 방식**: GitHub Fork 후 개인 저장소의 URL을 아래 Google Form에 제출  
  - 제출 Form URL: [https://forms.gle/6hR7mD59G7o1U2ZCA](https://forms.gle/6hR7mD59G7o1U2ZCA)  
  - 반드시 **Public 권한**으로 설정해주세요.

### 🎯 과제 목적

캐주얼 퍼즐 게임 클라이언트 개발자의 전반적인 기술 역량을 평가합니다:

- 설계 및 구조화 능력
- 커스텀 에디터 제작 능력
- 물리 및 셰이더에 대한 이해
- 프로토타입 프로젝트의 리팩토링 능력

### 📝평가 기준
- 코드 구조 및 설계 (40%) 
- 에디터 기능성 및 사용성 (40%) 
- 셰이더 구현 및 시각 효과 (15%) 
- 코드 스타일 및 문서화 (5%)

---

## 🔧 개발 환경

- **Unity 버전**: Unity 6 (6000.0.40f1 이상)
- **지원 플랫폼**: Android / iOS
- **실행 방법**:  
  `Assets/Project/Scenes/GameScene` 실행 → Drag & Drop 조작으로 진행

---

## 📁 제공 기능 요약

- `async/await` 기반 Level Board Generator
- 3D Physics + Joint 기반 블록 물리 처리
- Drag & Drop 블록 배치 기능
- DOTween을 활용한 연출
- 버텍스 셰이더 기반 Outline 처리
- 블록 파괴 연출 (`Quad` 기반 시각 트릭 사용 중)

---

## 01. 🛠️ Code Refactoring

### 🔍 대상 파일/폴더

- `Scripts` 폴더 내 전체 스크립트
- `Editor/BoardSOCreator.cs`
- `Project` 내 리소스 폴더 및 관련 데이터  
  (`Data`, `Materials`, `Model`, `Prefabs`, `Scenes/GameScene`, 등)

### 📌 요구 사항

- MVC 또는 MVP 패턴 기반으로 입력, 상태, 렌더링 로직 분리 및 최적화
- `BoardController`의 역할을 세분화하여 서브 컨트롤러 또는 헬퍼 클래스로 분리
- `BlockDragHandler`의 이벤트 처리와 물리 로직을 적절히 분리
- 기존 데이터 구조의 활용 또는 스테이지 확장성과 가독성을 고려한 자유로운 개선 가능

---

## 02. 🧩 Stage Editor 구현

### 💡 선택 구현 방식

- Unity Custom Editor (Inspector 또는 EditorWindow)
- Unity UI Toolkit (UXML 기반)
- In-Game Build (Mac/Win에서 실행 가능한 형태)

> (선택 사항) JSON 또는 ScriptableObject로의 데이터 변환/저장 기능 구현

### 📌 요구 사항

- 비개발자도 쉽게 사용할 수 있는 UI/UX 제공
- 블록 배치 및 색상 설정 기능
- Gimmick 정보 설정 (예: Star, Lock 등) 및 향후 기획 확장 고려
- Wall 및 출구 설정 기능 포함
- Editor Play 기능 포함

---

## 03. ✨ Visual Effect 최적화

### 🎯 요구 사항

- 기존 `Quad`를 이용한 가림 처리 방식 → **Stencil Buffer 기반 셰이더**로 전환
- 실제 오브젝트가 아닌 **버텍스 기반 스텐실**로 구현

### ➕ 추가 고려 사항

- VContainer, Addressables 연동 고려
- ScriptableObject(Event/Config) 기반 확장성 구조 적용
- MonoBehaviour 의존도 최소화 및 Interface 기반 추상화 지향
- 일관된 코딩 스타일 및 네이밍 유지
- AI 도구(GPT, Claude 등) 사용 가능하나 **코드 및 설계의 일관성과 이해도 필수**

---

## ✅ 유의 사항

- 평가 기준은 설계/구조화, 도구 완성도, 시각 효과 구현력, 코드 스타일을 종합적으로 고려합니다.
- 문서화 및 주석 처리도 중요합니다. 협업을 염두에 둔 코드 작성이 필요합니다.

---

## 📬 제출

- 제출 기한: **2025년 5월 14일 (화) 13:00**
- 제출 방법: 개인 GitHub 저장소(Fork) → Google Form 제출  
  (반드시 **public repository**로 설정)

---
