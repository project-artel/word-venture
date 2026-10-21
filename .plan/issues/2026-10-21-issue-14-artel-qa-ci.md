# 2026-10-21 — Linux 빌드에 ARTEL TC 검증 추가

- Date: 2026-10-21
- GitHub Issue: #14
- Status: In progress

## Goal

Linux Development Build를 만들고 ARTEL test-run을 실행해 CI 결과에 반영한다.

## Non-goals

기존 WebGL 배포, 게임 동작, 새로운 TC 작성, Unity license 발급은 변경하지 않는다.

## Context / Constraints

- 사용자 승인으로 main에서 develop을 만들었다. 작업 PR은 develop을 대상으로 한다.
- Unity 2022.3.62f3과 기존 GameCI 빌드 방식을 유지한다.
- SDK의 외부 local path는 공개 Git commit으로 고정한다.
- ARTEL project 11 / test-run 28을 사용한다. ARTEL_TOKEN과 UNITY_LICENSE를 등록했다.
  UNITY_EMAIL과 UNITY_PASSWORD도 사용자 등록을 확인했다.
- 사용자 요청으로 Windows 실행을 취소하고 Linux 빌드와 Xvfb/Mesa 렌더링으로 전환했다.
- qa-action alpha는 runner의 실제 게임 렌더링을 보장하지 않는다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 기존 workflow, SDK manifest, 인증 상태 확인.
- [x] **Step 1: Implementation** — SDK Git dependency, Linux Development Build method,
  preflight → build → qa workflow, 설정 문서 추가.
- [ ] **Step 2: Tests** — JSON/YAML, actionlint, workflow 연결 검사 통과. 인증 설정 후 실제 CI 실행.
- [ ] **Step 3: Rollout / Rollback** — develop 대상 draft PR. 변경 revert로 제거 가능.

## Validation

- **Commands to run:** JSON/YAML parser, actionlint, git diff --check, GitHub Actions.
- **Expected output:** 누락 설정은 preflight에서 실패. 준비된 환경에서는 빌드 artifact를
  Linux runner가 내려받고 qa-action의 판정과 결과 artifact를 남긴다.

## Risks & Rollback

- **Risks:** Unity license 및 실제 graphics 지원 미확인. 공개 SDK 0.2.0의 Unity compilation 필요.
- **Rollback steps:** 이 변경을 revert한다. 기존 WebGL/release workflow는 그대로 유지한다.

## Open Questions

- 로컬 license의 runner 활성화와 Linux software rendering에서의 TC 실행 확인이 남았다.
