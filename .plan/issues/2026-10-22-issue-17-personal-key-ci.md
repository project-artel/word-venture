# 개인 Sonnet key와 planner_executer CI 연결

- Issue: #17
- Goal: 개인 key를 사용하는 Sonnet과 planner_executer 조합으로 TC 검증을 실행한다.
- Changes: qa-action key-source=user 입력, 새 Action SHA, 기본 planner_executer 구조.
- Validation: Action 테스트 21개, actionlint, 실제 CI 및 ARTEL 실행 기록을 확인한다.
- Rollback: workflow 변경 commit을 revert한다. 개인 모델 key 원문은 GitHub로 보내지 않는다.