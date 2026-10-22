# 개인 Sonnet key와 planner_executer CI 연결

- Issue: #17
- Goal: 개인 key를 사용하는 Sonnet과 planner_executer 조합으로 TC 검증을 실행한다.
- Changes: qa-action key-source=user 입력, 새 Action SHA, 기본 planner_executer 구조, 출력 language 기본 ko 및 ko/en 수동 선택.
- Validation: Action 테스트 23개 통과. run 136은 개인 key로 Sonnet 46회 호출 후 FAILED, 최종 TC 판정 없음. planner 언어 수정의 stage 배포와 화면 반전 확인 전 release하지 않는다.
- Rollback: workflow 변경 commit을 revert한다. 개인 모델 key 원문은 GitHub로 보내지 않는다.
