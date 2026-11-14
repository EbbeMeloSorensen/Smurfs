namespace Smurfs.UI.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private readonly Button loadButton = new() { Text = "Load smurfs", AutoSize = true };
    private readonly Button previousButton = new() { Text = "Previous", AutoSize = true };
    private readonly Button nextButton = new() { Text = "Next", AutoSize = true };
    private readonly Label pageStatus = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label errorStatus = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly ListBox smurfs = new() { Dock = DockStyle.Fill, IntegralHeight = false };

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Text = "Smurfs.UI.WinForms";

        var toolbar = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        toolbar.Controls.AddRange([loadButton, previousButton, pageStatus, nextButton]);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(errorStatus, 0, 1);
        layout.Controls.Add(smurfs, 0, 2);
        Controls.Add(layout);
    }

    #endregion
}
