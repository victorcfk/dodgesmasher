using UnityEngine;
using UnityEditor;

/// <summary>
/// This EditorWindow can recieve and send Modal inputs.
/// </summary>
public interface IModal
{
	/// <summary>
	/// Called when the Modal shortcut is pressed.
	/// The implementation should call Create if the condition are right.
	/// </summary>
	void ModalRequest(bool shift);

	/// <summary>
	/// Called when the associated modal is closed.
	/// </summary>
	void ModalClosed(ModalWindow window);
}

public enum WindowResult
{
	None,
	Ok,
	Cancel,
	Invalid,
	LostFocus
}

/// <summary>
/// Define a popup window that return a result.
/// Base class for IModal call implementation.
/// </summary>
public abstract class ModalWindow : EditorWindow
{
	public const float TITLEBAR = 18;

	protected IModal owner;

	protected WindowResult result = WindowResult.None;

	public WindowResult Result
	{
		get { return result; }
	}

	protected virtual void OnLostFocus()
	{
		result = WindowResult.LostFocus;

		if (owner != null)
			owner.ModalClosed(this);
	}

	protected virtual void Cancel()
	{
		result = WindowResult.Cancel;

		if (owner != null)
			owner.ModalClosed(this);

		Close();
	}

	protected virtual void Ok()
	{
		result = WindowResult.Ok;

		if (owner != null)
			owner.ModalClosed(this);

		Close();
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
		GUILayout.BeginHorizontal(EditorStyles.toolbar);

		GUILayout.Label(titleContent);

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		Rect content = new Rect(0, TITLEBAR, position.width, position.height - TITLEBAR);
		Draw(content);
	}

	protected abstract void Draw(Rect region);
}

/// <summary>
/// The rename popup is a generic popup that allow the user to input a name or to rename an existing one.
/// You can pass a delegate to valide the currently input string.
/// </summary>
public class RenameStringModalWindow : ModalWindow
{
	public delegate bool ValidateName(string name);

	public const float BUTTONS_HEIGHT = 30;
	public const float FIELD_HEIGHT = 20;
	public const float HEIGHT = 56;
	public const float WIDTH = 250;
	bool firstFrame;

	private string[] texts;
	public string[] Texts
	{
		get { return texts; }
	}

	private ValidateName[] validate;

	public static RenameStringModalWindow Create(IModal owner, string title, string[] texts, Vector2 position)
	{
		return Create(owner, title, texts, position, null);
	}

	public static RenameStringModalWindow Create(IModal owner, string title, string[] texts, Vector2 position, ValidateName[] validate)
	{
		RenameStringModalWindow rename = ScriptableObject.CreateInstance<RenameStringModalWindow>();

		rename.owner = owner;
		rename.titleContent = new GUIContent(title);
		rename.texts = texts;
		rename.validate = validate;
		
		float height = HEIGHT + (texts.Length * FIELD_HEIGHT);

		float x = position.x - WIDTH / 2;
		float y = position.y - height / 2;
		
		Rect rect = new Rect(x, y, 0, 0);
		rename.position = rect;
		rename.ShowAsDropDown(rect, new Vector2(WIDTH, height));
		rename.firstFrame = true;

		return rename;
	}

	protected override void Draw(Rect region)
	{
		bool valid = true;

		if (validate != null)
		{
			for (int i = 0; i < validate.Length; i++)
			{
				if (validate[i] != null && !validate[i](texts[i]))
				{
					valid = false;
					break;
				}
			}
		}

		if (Event.current.type == EventType.KeyDown)
		{
			if (Event.current.keyCode == KeyCode.Return && valid)
				Ok();

			if (Event.current.keyCode == KeyCode.Escape)
				Cancel();
		}

		GUILayout.BeginArea(region);

		GUILayout.Space(5);

		for (int i = 0; i < texts.Length; i++)
		{
			GUILayout.BeginHorizontal();

			if (validate != null && validate[i] != null)
				valid = validate[i](texts[i]);

			if (valid)
			{
				GUI.color = Color.green;
				GUILayout.Label(new GUIContent(" ✔"), GUILayout.Width(18), GUILayout.Height(18));
			}
			else
			{
				valid = false;
				GUI.color = Color.red;
				GUILayout.Label(new GUIContent(" ✘"), GUILayout.Width(18), GUILayout.Height(18));
			}

			GUI.color = Color.white;
			if (i == 0) GUI.SetNextControlName("FirstTextField");
			texts[i] = EditorGUILayout.TextField(texts[i]);
			if (i == 0 && firstFrame) EditorGUI.FocusTextInControl("FirstTextField");

			GUILayout.EndHorizontal();
		}
		firstFrame = false;

		GUILayout.Space(5);

		GUILayout.BeginHorizontal();

		EditorGUI.BeginDisabledGroup(!valid);

		if (GUILayout.Button("Ok"))
			Ok();

		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button("Cancel"))
			Cancel();

		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
