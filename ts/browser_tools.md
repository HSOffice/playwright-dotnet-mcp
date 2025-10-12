## Summary
- Cataloged every `browser_*` tool defined under `ts/mcp/browser/tools`, including each tool’s capability, type, and detailed input schema parameter descriptions.

## Tool details
#### `browser_close` — ts/mcp/browser/tools/common.ts
- Title: Close browser
- Description: Close the page.
- Capability: core
- Type: action
- Input schema: empty object (no parameters).

#### `browser_resize` — ts/mcp/browser/tools/common.ts
- Title: Resize browser window
- Description: Resize the browser window.
- Capability: core
- Type: action
- Input schema:
  - `width` (number): “Width of the browser window.”
  - `height` (number): “Height of the browser window.”

#### `browser_console_messages` — ts/mcp/browser/tools/console.ts
- Title: Get console messages
- Description: Returns all console messages.
- Capability: core
- Type: readOnly
- Input schema:
  - `onlyErrors` (optional boolean): “Only return error messages.”

#### `browser_handle_dialog` — ts/mcp/browser/tools/dialogs.ts
- Title: Handle a dialog
- Description: Handle a dialog.
- Capability: core
- Type: action
- Input schema:
  - `accept` (boolean): “Whether to accept the dialog.”
  - `promptText` (optional string): “The text of the prompt in case of a prompt dialog.”

#### `browser_evaluate` — ts/mcp/browser/tools/evaluate.ts
- Title: Evaluate JavaScript
- Description: Evaluate JavaScript expression on page or element.
- Capability: core
- Type: action
- Input schema:
  - `function` (string): “() => { /* code */ } or (element) => { /* code */ } when element is provided.”
  - `element` (optional string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (optional string): “Exact target element reference from the page snapshot.”

#### `browser_file_upload` — ts/mcp/browser/tools/files.ts
- Title: Upload files
- Description: Upload one or multiple files.
- Capability: core
- Type: action
- Input schema:
  - `paths` (optional array of strings): “The absolute paths to the files to upload. Can be single file or multiple files. If omitted, file chooser is cancelled.”

#### `browser_fill_form` — ts/mcp/browser/tools/form.ts
- Title: Fill form
- Description: Fill multiple form fields.
- Capability: core
- Type: input
- Input schema:
  - `fields` (array): “Fields to fill in.” Each item has:
    - `name` (string): “Human-readable field name.”
    - `type` (enum textbox/checkbox/radio/combobox/slider): “Type of the field.”
    - `ref` (string): “Exact target field reference from the page snapshot.”
    - `value` (string): “Value to fill in the field. If the field is a checkbox, the value should be `true` or `false`. If the field is a combobox, the value should be the text of the option.”

#### `browser_press_key` — ts/mcp/browser/tools/keyboard.ts
- Title: Press a key
- Description: Press a key on the keyboard.
- Capability: core
- Type: input
- Input schema:
  - `key` (string): “Name of the key to press or a character to generate, such as `ArrowLeft` or `a`.”

#### `browser_type` — ts/mcp/browser/tools/keyboard.ts
- Title: Type text
- Description: Type text into editable element.
- Capability: core
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (string): “Exact target element reference from the page snapshot.”
  - `text` (string): “Text to type into the element.”
  - `submit` (optional boolean): “Whether to submit entered text (press Enter after).”
  - `slowly` (optional boolean): “Whether to type one character at a time. Useful for triggering key handlers in the page. By default entire text is filled in at once.”

#### `browser_install` — ts/mcp/browser/tools/install.ts
- Title: Install the browser specified in the config
- Description: Install the browser specified in the config. Call this if you get an error about the browser not being installed.
- Capability: core-install
- Type: action
- Input schema: empty object (no parameters).

#### `browser_navigate` — ts/mcp/browser/tools/navigate.ts
- Title: Navigate to a URL
- Description: Navigate to a URL.
- Capability: core
- Type: action
- Input schema:
  - `url` (string): “The URL to navigate to.”

#### `browser_navigate_back` — ts/mcp/browser/tools/navigate.ts
- Title: Go back
- Description: Go back to the previous page.
- Capability: core
- Type: action
- Input schema: empty object (no parameters).

#### `browser_network_requests` — ts/mcp/browser/tools/network.ts
- Title: List network requests
- Description: Returns all network requests since loading the page.
- Capability: core
- Type: readOnly
- Input schema: empty object (no parameters).

#### `browser_pdf_save` — ts/mcp/browser/tools/pdf.ts
- Title: Save as PDF
- Description: Save page as PDF.
- Capability: pdf
- Type: readOnly
- Input schema:
  - `filename` (optional string): “File name to save the pdf to. Defaults to `page-{timestamp}.pdf` if not specified.”

#### `browser_take_screenshot` — ts/mcp/browser/tools/screenshot.ts
- Title: Take a screenshot
- Description: “Take a screenshot of the current page. You can't perform actions based on the screenshot, use browser_snapshot for actions.”
- Capability: core
- Type: readOnly
- Input schema:
  - `type` (enum png/jpeg, default `png`): “Image format for the screenshot. Default is png.”
  - `filename` (optional string): “File name to save the screenshot to. Defaults to `page-{timestamp}.{png|jpeg}` if not specified.”
  - `element` (optional string): “Human-readable element description used to obtain permission to screenshot the element. If not provided, the screenshot will be taken of viewport. If element is provided, ref must be provided too.”
  - `ref` (optional string): “Exact target element reference from the page snapshot. If not provided, the screenshot will be taken of viewport. If ref is provided, element must be provided too.”
  - `fullPage` (optional boolean): “When true, takes a screenshot of the full scrollable page, instead of the currently visible viewport. Cannot be used with element screenshots.”

#### `browser_snapshot` — ts/mcp/browser/tools/snapshot.ts
- Title: Page snapshot
- Description: Capture accessibility snapshot of the current page, this is better than screenshot.
- Capability: core
- Type: readOnly
- Input schema: empty object (no parameters).

#### `browser_click` — ts/mcp/browser/tools/snapshot.ts
- Title: Click
- Description: Perform click on a web page.
- Capability: core
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (string): “Exact target element reference from the page snapshot.”
  - `type` (enum button/link/checkbox/radio/switch/combobox/menuitem/menuitemcheckbox/menuitemradio/option/tab/treeitem/listitem/slider/textbox): “Type of the field.”
  - `doubleClick` (optional boolean): “Whether to perform a double click instead of a single click.”
  - `button` (optional enum left/right/middle): “Button to click, defaults to left.”
  - `modifiers` (optional array of enums Alt/Control/ControlOrMeta/Meta/Shift): “Modifier keys to press.”

#### `browser_drag` — ts/mcp/browser/tools/snapshot.ts
- Title: Drag mouse
- Description: Perform drag and drop between two elements.
- Capability: core
- Type: input
- Input schema:
  - `startElement` (string): “Human-readable source element description used to obtain the permission to interact with the element.”
  - `startRef` (string): “Exact source element reference from the page snapshot.”
  - `endElement` (string): “Human-readable target element description used to obtain the permission to interact with the element.”
  - `endRef` (string): “Exact target element reference from the page snapshot.”

#### `browser_hover` — ts/mcp/browser/tools/snapshot.ts
- Title: Hover mouse
- Description: Hover over element on page.
- Capability: core
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (string): “Exact target element reference from the page snapshot.”

#### `browser_select_option` — ts/mcp/browser/tools/snapshot.ts
- Title: Select option
- Description: Select an option in a dropdown.
- Capability: core
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (string): “Exact target element reference from the page snapshot.”
  - `values` (array of strings): “Array of values to select in the dropdown. This can be a single value or multiple values.”

#### `browser_generate_locator` — ts/mcp/browser/tools/snapshot.ts
- Title: Create locator for element
- Description: Generate locator for the given element to use in tests.
- Capability: testing
- Type: readOnly
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `ref` (string): “Exact target element reference from the page snapshot.”

#### `browser_mouse_move_xy` — ts/mcp/browser/tools/mouse.ts
- Title: Move mouse
- Description: Move mouse to a given position.
- Capability: vision
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `x` (number): “X coordinate.”
  - `y` (number): “Y coordinate.”

#### `browser_mouse_click_xy` — ts/mcp/browser/tools/mouse.ts
- Title: Click
- Description: Click left mouse button at a given position.
- Capability: vision
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `x` (number): “X coordinate.”
  - `y` (number): “Y coordinate.”

#### `browser_mouse_drag_xy` — ts/mcp/browser/tools/mouse.ts
- Title: Drag mouse
- Description: Drag left mouse button to a given position.
- Capability: vision
- Type: input
- Input schema:
  - `element` (string): “Human-readable element description used to obtain permission to interact with the element.”
  - `startX` (number): “Start X coordinate.”
  - `startY` (number): “Start Y coordinate.”
  - `endX` (number): “End X coordinate.”
  - `endY` (number): “End Y coordinate.”

#### `browser_tabs` — ts/mcp/browser/tools/tabs.ts
- Title: Manage tabs
- Description: List, create, close, or select a browser tab.
- Capability: core-tabs
- Type: action
- Input schema:
  - `action` (enum list/new/close/select): “Operation to perform.”
  - `index` (optional number): “Tab index, used for close/select. If omitted for close, current tab is closed.”

#### `browser_start_tracing` — ts/mcp/browser/tools/tracing.ts
- Title: Start tracing
- Description: Start trace recording.
- Capability: tracing
- Type: readOnly
- Input schema: empty object (no parameters).

#### `browser_stop_tracing` — ts/mcp/browser/tools/tracing.ts
- Title: Stop tracing
- Description: Stop trace recording.
- Capability: tracing
- Type: readOnly
- Input schema: empty object (no parameters).

#### `browser_wait_for` — ts/mcp/browser/tools/wait.ts
- Title: Wait for
- Description: Wait for text to appear or disappear or a specified time to pass.
- Capability: core
- Type: assertion
- Input schema:
  - `time` (optional number): “The time to wait in seconds.”
  - `text` (optional string): “The text to wait for.”
  - `textGone` (optional string): “The text to wait for to disappear.”

#### `browser_verify_element_visible` — ts/mcp/browser/tools/verify.ts
- Title: Verify element visible
- Description: Verify element is visible on the page.
- Capability: testing
- Type: assertion
- Input schema:
  - `role` (string): “ROLE of the element. Can be found in the snapshot like this: `- {ROLE} "Accessible Name":`.”
  - `accessibleName` (string): “ACCESSIBLE_NAME of the element. Can be found in the snapshot like this: `- role "{ACCESSIBLE_NAME}"`.”

#### `browser_verify_text_visible` — ts/mcp/browser/tools/verify.ts
- Title: Verify text visible
- Description: “Verify text is visible on the page. Prefer browser_verify_element_visible if possible.”
- Capability: testing
- Type: assertion
- Input schema:
  - `text` (string): “TEXT to verify. Can be found in the snapshot like this: `- role "Accessible Name": {TEXT}` or like this: `- text: {TEXT}`.”

#### `browser_verify_list_visible` — ts/mcp/browser/tools/verify.ts
- Title: Verify list visible
- Description: Verify list is visible on the page.
- Capability: testing
- Type: assertion
- Input schema:
  - `element` (string): “Human-readable list description.”
  - `ref` (string): “Exact target element reference that points to the list.”
  - `items` (array of strings): “Items to verify.”

#### `browser_verify_value` — ts/mcp/browser/tools/verify.ts
- Title: Verify value
- Description: Verify element value.
- Capability: testing
- Type: assertion
- Input schema:
  - `type` (enum textbox/checkbox/radio/combobox/slider): “Type of the element.”
  - `element` (string): “Human-readable element description.”
  - `ref` (string): “Exact target element reference that points to the element.”
  - `value` (string): “Value to verify. For checkbox, use `true` or `false`.”
