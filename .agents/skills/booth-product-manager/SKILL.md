---
name: booth-product-manager
description: Operate BOOTH shop product creation and editing with playwright-cli browser automation for avatar distribution work. Use when Codex needs to add or edit BOOTH products, update product settings, replace product images or thumbnails, upload distribution files, or rehearse those BOOTH workflows safely without publishing live changes.
---

# BOOTH Product Manager

Use this skill in `C:\Users\wyndf\Documents\unity\Avaters` when operating BOOTH product admin pages through `playwright-cli`.

## Safety Rules

- Treat BOOTH as a live commerce/admin system. Do not click final publish, release, sell, or save-and-publish controls unless the user explicitly asks for it in the current turn.
- For tests, keep the product unpublished/private/draft and include `Codex Test` and the date in the title.
- Before every write action, inspect the page with `npx playwright-cli -s=booth snapshot`.
- After editing each field or upload input, verify the value or visible result before moving on.
- BOOTH shows a browser `Leave site?` confirmation when it detects unsaved edits. Avoid page navigation, back, tab close, or product-list links until a draft save has completed and the save confirmation is visible.
- If a button label is ambiguous between saving a draft and publishing, stop and ask the user.
- Do not delete or overwrite existing user products during testing. Create a separate test product or edit only a product URL/ID that the user explicitly provides as a test target.
- Do not invent product files. Use files under `Products/<AvatarName>/` only after confirming they exist.

## Prerequisites

1. Confirm `playwright-cli` is available:

```powershell
npx playwright-cli --version
```

2. Load local automation secrets from `.env` before using `playwright-cli`. Do this in each new PowerShell session:

```powershell
Get-Content .env | ForEach-Object {
  if ($_ -match '^\s*([^#][^=]+)=(.*)$') {
    [Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2].Trim(), 'Process')
  }
}
```

3. Attach to the user's normal Chrome through the Playwright extension:

```powershell
npx playwright-cli -s=booth attach --extension=chrome
npx playwright-cli -s=booth tab-list
```

4. Confirm the attached browser is logged into the BOOTH account that owns the target shop.
5. If the current tab is not BOOTH, navigate safely:

```powershell
npx playwright-cli -s=booth goto "https://manage.booth.pm/"
npx playwright-cli -s=booth snapshot
```

## Repository Inputs

Before uploading or replacing media, inspect the product folder:

```powershell
Get-ChildItem -LiteralPath "Products\<AvatarName>" -Force
Get-ChildItem -LiteralPath "Products\<AvatarName>\booth" -Force
```

Expected local layout:

- `Products/<AvatarName>/<AvatarName>.zip`: distribution package.
- `Products/<AvatarName>/<AvatarName>.unitypackage`: Unity import package.
- `Products/<AvatarName>/README.txt`: product/readme text when available.
- `Products/<AvatarName>/booth/1.png`, `2.png`, etc.: BOOTH images in display order.
- `Products/<AvatarName>/booth/Assets/`: source materials for thumbnails.

## Create A Non-Published Test Product

1. Attach to Chrome with `npx playwright-cli -s=booth attach --extension=chrome`.
2. Open `https://manage.booth.pm/` with session `booth`.
3. Navigate to the product creation page using visible dashboard links. Prefer numeric refs from `state`; use AX state for icon-only controls.
4. Fill the minimum safe fields:
   - Product name: `Codex Test Avatar Product YYYY-MM-DD HHmm`
   - Description: short text explaining this is an unpublished automation test.
   - Price: use `0` only if BOOTH allows it for the selected product type; otherwise use the lowest safe test value and keep the item unpublished.
   - Category/type: choose the closest avatar/digital/downloadable product type available in the UI.
5. Upload only temporary or explicitly approved files. For avatar package tests, prefer a small harmless text/zip fixture if available; otherwise stop and ask before uploading real distribution files.
6. Upload images from `Products/<AvatarName>/booth/*.png` only when the user asked to test image/thumbnail upload and the files exist.
7. Choose draft/private/unpublished visibility. Verify visible status text.
8. Save as draft only. Do not publish. Wait for `商品を保存しました` or the current BOOTH save-success text before navigating away.
9. Capture final state and URL:

```powershell
npx playwright-cli -s=booth eval "location.href"
npx playwright-cli -s=booth snapshot
```

Report the product title, draft/private status, and the edit URL.

## Edit A Test Product

1. Start from a user-provided test product edit URL or the test product created in the same session.
2. Confirm the page title/product name contains a test marker such as `Codex Test`.
3. For setting updates, change one field at a time and verify with `get value`, `get text`, or fresh `state`.
4. For image changes:
   - Inspect existing image slots and upload controls.
   - Upload files in numeric order from `Products/<AvatarName>/booth/`.
   - Verify the visible order after upload/reorder.
5. For thumbnail changes:
   - Identify whether BOOTH uses the first product image or a separate thumbnail/crop control.
   - If there is a crop/position dialog, use screenshot annotation before clicking visual controls.
   - Verify the selected thumbnail preview changed.
6. For file changes:
   - Inspect the accepted file types from the file input compound metadata.
   - Upload the requested package file from `Products/<AvatarName>/`.
   - Verify the displayed filename and size.
7. Save as draft/private only, wait for the save-success modal/text, then re-read the page state.

## Tags

Recommended tag buttons can be clicked directly and are reliable with playwright-cli:

```powershell
npx playwright-cli -s=booth click <Quest対応-button-ref>
npx playwright-cli -s=booth click <VRChat想定モデル-button-ref>
npx playwright-cli -s=booth click <VRChat-button-ref>
npx playwright-cli -s=booth eval "document.querySelector('#item_tag')?.innerText"
```

Verify the selected tag names appear before `おすすめタグ（履歴）:`. Save the draft after tag changes.

## Uploads

As of the BOOTH edit UI tested on 2026-05-17, file inputs are created lazily and hidden.

For downloadable work files, do not click the generated file input. Use playwright-cli file drop against the visible BOOTH drop zone. This was verified on 2026-05-17 against test product `8372177`, uploading `Products\Catchy\Catchy.zip` and saving as draft.

```powershell
npx playwright-cli --version
Get-Content .env | ForEach-Object {
  if ($_ -match '^\s*([^#][^=]+)=(.*)$') {
    [Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2].Trim(), 'Process')
  }
}
npx playwright-cli -s=booth attach --extension=chrome
npx playwright-cli -s=booth goto "https://manage.booth.pm/items/<ItemId>/edit"
npx playwright-cli -s=booth snapshot
```

Before replacing or adding images, compute local hashes and compare against the current intended source list so unchanged images are skipped:

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath "Products\<AvatarName>\booth\*.png"
```

Open the file manager, drop the local file onto the BOOTH drop zone, and verify the file appears:

```powershell
npx playwright-cli -s=booth click <ファイルの追加・管理-ref>
npx playwright-cli -s=booth drop <ファイルをドラッグ＆ドロップ-container-ref> --path="C:\Users\wyndf\Documents\unity\Avaters\Products\<AvatarName>\<AvatarName>.zip"
```

Then wait/verify:

```powershell
npx playwright-cli -s=booth run-code "async page => {
  const fileName = '<AvatarName>.zip';
  await page.getByText(fileName).waitFor({ timeout: 180000 });
  return await page.evaluate(name => ({
    hasFile: document.body.innerText.includes(name),
    hasUploadedHeader: document.body.innerText.includes('アップロードしたファイル'),
    hasNoFileWarning: document.body.innerText.includes('作品ファイルの無い')
  }), fileName);
}"
```

After the file appears, close the file manager and save the draft only:

```powershell
npx playwright-cli -s=booth click <閉じる-ref>
npx playwright-cli -s=booth click <下書きで保存する-ref>
npx playwright-cli -s=booth snapshot
```

Confirm that the page shows both the uploaded filename and `商品を保存しました`. If the save-success modal appears, click `編集を続ける` to stay on the edit page.

## BOOTH Unsaved-Change Guard

BOOTH edit pages may block automation with a browser confirmation dialog if a command tries to leave a page with unsaved edits. Use this discipline:

1. Keep the session on the edit URL until the draft save is confirmed.
2. Do not click header/sidebar links, browser back, product preview links, or `商品管理へ` while edits are dirty.
3. If a save modal appears, prefer `編集を続ける` to remain on the edit page. Use `商品管理へ` only after save confirmation and only when you intentionally want to leave.
4. If refs go stale around the save buttons, take a fresh snapshot and click by the new ref or role/name:

```powershell
npx playwright-cli -s=booth snapshot
npx playwright-cli -s=booth click <下書きで保存する-ref>
npx playwright-cli -s=booth run-code "async page => await page.getByText('商品を保存しました').waitFor({ timeout: 20000 })"
```

5. If the browser confirmation is already shown, cancel/keep editing, then save the draft before any further navigation.

## BOOTH React Field Input

Some BOOTH React fields can reject `browser fill`, `browser type`, or clipboard paste. When verified normal input fails, set values with the native DOM setter and dispatch `input`/`change` events, then immediately verify and save:

```powershell
$js = @'
(() => {
  const setValue = (el, value) => {
    const proto = el instanceof HTMLTextAreaElement
      ? HTMLTextAreaElement.prototype
      : HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value').set;
    setter.call(el, value);
    el.dispatchEvent(new InputEvent('input', { bubbles: true, inputType: 'insertText', data: value }));
    el.dispatchEvent(new Event('change', { bubbles: true }));
  };
  setValue(document.querySelector('#name input'), 'Codex Test Avatar Product YYYY-MM-DD HHmm');
  setValue(document.querySelector('#description textarea'), 'Unpublished automation test.');
  setValue(document.querySelector('input[name="price"]'), '100');
  return {
    name: document.querySelector('#name input')?.value,
    description: document.querySelector('#description textarea')?.value,
    price: document.querySelector('input[name="price"]')?.value
  };
})()
'@
npx playwright-cli -s=booth eval $js
```

## playwright-cli Interaction Pattern

Use this loop for each page:

```powershell
npx playwright-cli -s=booth snapshot
npx playwright-cli -s=booth click <ref>
npx playwright-cli -s=booth run-code "async page => await page.getByText('...').waitFor({ timeout: 15000 })"
npx playwright-cli -s=booth snapshot
```

Use file uploads through `drop --path` against the visible drop target:

```powershell
npx playwright-cli -s=booth drop <drop-zone-ref> --path="C:\absolute\path\to\file.zip"
npx playwright-cli -s=booth snapshot
```

Use request inspection only to understand page behavior or draft-save APIs. Do not replay BOOTH write requests manually unless the user explicitly asks for adapter development and the request is safe.

## Failure Handling

- If login, 2FA, CAPTCHA, payment, seller verification, or account confirmation appears, stop and let the user complete it.
- If a ref is stale on a destructive or publish-adjacent control, take a fresh snapshot and re-confirm the target.
- If the UI lacks a safe draft/private option, stop before saving.
- If an upload succeeds but the visible file/image does not update, do not retry more than once without inspecting state and logs.
- Keep this workflow as playwright-cli driven admin automation unless the user explicitly asks for another automation backend.
