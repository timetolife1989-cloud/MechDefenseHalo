# Branch Strategy

## Main Branch
- `main` - production-ready code
- Protected: requires passing CI
- All merges are squash commits
- History remains clean and linear

## Feature Branches
- **Format:** `feature/descriptive-name`
- **Always branch from:** `main` (never from other feature branches)
- **Never stack PRs:** No `feature/A` → `feature/B` dependencies
- **Auto-deleted after merge:** Branches are cleaned up automatically

## Workflow

### Creating a Feature Branch
```bash
# 1. Update main branch
git checkout main
git pull origin main

# 2. Create feature branch from main
git checkout -b feature/your-feature-name

# 3. Make your changes
# ... code changes ...

# 4. Commit and push
git add .
git commit -m "Implement your feature"
git push origin feature/your-feature-name
```

### Opening a Pull Request
1. Open PR from your feature branch to `main`
2. Fill out the PR template
3. CI automatically runs validation
4. If build passes → Auto-merge triggers
5. If build fails → Fix errors and push again

### What Happens Automatically
- ✅ **CI Validation:** Build and test on every push
- ✅ **Auto-merge:** Merges automatically when CI passes
- ✅ **Squash merge:** All commits squashed into one
- ✅ **Branch deletion:** Feature branch deleted after merge
- ✅ **No manual approval:** Full trust - CI validation only

## Branch Protection Rules
The `main` branch requires:
1. ✅ Passing CI checks (build + tests)
2. ❌ No manual approval required
3. ✅ Up-to-date with base branch
4. ✅ Linear history (squash merges only)

## Independent Branch Strategy
Each PR is **completely independent**:
- 🚫 No stacking (branch dependencies)
- 🚫 No waiting for other PRs
- ✅ Always branch from latest `main`
- ✅ Parallel execution (up to 20 PRs)
- ✅ No merge conflicts between PRs

## Handling Conflicts
If your branch has conflicts with `main`:
```bash
# Update your feature branch
git checkout feature/your-feature-name
git fetch origin
git merge origin/main

# Resolve conflicts
# ... resolve conflicts in files ...

git add .
git commit -m "Resolve merge conflicts"
git push origin feature/your-feature-name
```

## Branch Naming Conventions
- `feature/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions/updates

## Examples
✅ **Good:**
```
main → feature/add-user-auth
main → feature/improve-performance
main → fix/login-bug
```

❌ **Bad:**
```
feature/add-auth → feature/add-oauth  # No stacking!
main → dev → feature/new-thing        # No dev branch!
```

## CI/CD Pipeline
1. **PR Opened** → CI validation starts
2. **Build Passes** → Auto-merge enabled
3. **All Checks Pass** → PR merged automatically
4. **Branch Deleted** → Cleanup complete

## Key Principles
1. **Always branch from main** - Keep it simple
2. **No dependencies** - Each PR stands alone
3. **Trust the CI** - No manual approvals
4. **Fast iteration** - Merge quickly when green
5. **Clean history** - Squash commits on merge
