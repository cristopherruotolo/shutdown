using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimerApp
{
    public class MainForm : Form
    {
        // Componentes visuais e variáveis principais
        private Label lblCountdown; // Exibe o tempo restante
        private Button btnSetShutdown; // Botão para configurar o desligamento
        private Button btnCancelShutdown; // Botão para cancelar o desligamento
        private NotifyIcon trayIcon; // Ícone da bandeja do sistema
        private Timer countdownTimer; // Timer para contagem regressiva
        private int remainingTime = 0; // Tempo restante em segundos

        public MainForm()
        {
            InitializeComponents(); // Inicializa os componentes do formulário
        }

        private void InitializeComponents()
        {
            // Configuração inicial do formulário
            this.Text = "Shutdown Timer";
            this.Size = new Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Define o ícone do formulário
            this.Icon = new Icon("../../ampulheta.ico"); // Nome do ícone adicionado ao projeto

            // Rótulo para exibir a contagem regressiva
            lblCountdown = new Label()
            {
                Text = "00:00:00",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Botão para configurar o desligamento
            btnSetShutdown = new Button()
            {
                Text = "Programar para Desligar",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnSetShutdown.Click += BtnSetShutdown_Click;

            // Botão para cancelar o desligamento
            btnCancelShutdown = new Button()
            {
                Text = "Cancelar Desligamento",
                Dock = DockStyle.Top,
                Height = 40,
                Enabled = false
            };
            btnCancelShutdown.Click += BtnCancelShutdown_Click;

            // Timer para contagem regressiva
            countdownTimer = new Timer()
            {
                Interval = 1000
            };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Ícone da bandeja do sistema
            trayIcon = new NotifyIcon()
            {
                Icon = new Icon("../../ampulheta.ico"), // Ícone personalizado
                Visible = true,
                Text = "Shutdown Timer"
            };
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            // Menu de contexto para o ícone da bandeja
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Restaurar", null, (s, e) => RestoreWindow());
            trayMenu.Items.Add("Sair", null, (s, e) => ExitApplication());
            trayIcon.ContextMenuStrip = trayMenu;

            // Evento para minimizar o formulário
            this.Resize += MainForm_Resize;

            // Adiciona os componentes ao formulário
            this.Controls.Add(lblCountdown);
            this.Controls.Add(btnSetShutdown);
            this.Controls.Add(btnCancelShutdown);
        }


        private void BtnSetShutdown_Click(object sender, EventArgs e)
        {
            // Solicita o tempo em minutos para o desligamento
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Digite o tempo em minutos para desligar:",
                "Programar Desligamento",
                "10"
            );

            // Valida a entrada e inicia a contagem regressiva
            if (int.TryParse(input, out int minutes) && minutes > 0)
            {
                remainingTime = minutes * 60;
                lblCountdown.Text = TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss");
                countdownTimer.Start();
                btnCancelShutdown.Enabled = true; // Ativa o botão de cancelar desligamento
                try
                {
                    ExecuteShutdownCommand(remainingTime); // Configura o desligamento no sistema
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao programar o desligamento: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Por favor, insira um valor válido em minutos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelShutdown_Click(object sender, EventArgs e)
        {
            // Garante que o botão só funcione quando ativado
            if (!btnCancelShutdown.Enabled) return;

            try
            {
                countdownTimer.Stop(); // Para a contagem regressiva
                remainingTime = 0;
                lblCountdown.Text = "00:00:00"; // Reseta o contador

                ExecuteShutdownCommand(0, cancel: true); // Cancela o desligamento no sistema
                btnCancelShutdown.Enabled = false; // Desativa o botão após o cancelamento
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cancelar o desligamento: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            // Atualiza a contagem regressiva a cada segundo
            if (remainingTime > 0)
            {
                remainingTime--;
                lblCountdown.Text = TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss");
            }
            else
            {
                countdownTimer.Stop(); // Para o timer quando o tempo acaba
            }
        }

        private void ExecuteShutdownCommand(int seconds, bool cancel = false)
        {
            try
            {
                // Define o comando de desligamento ou cancelamento
                string command = cancel ? "/a" : $"/s /t {seconds}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C shutdown {command}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System)
                };

                Process process = Process.Start(startInfo);

                process.WaitForExit(); // Aguarda a execução do comando
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar o comando: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Minimiza o formulário para a bandeja do sistema
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                trayIcon.Visible = true;
            }
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreWindow(); // Restaura o formulário ao clicar duas vezes no ícone
        }

        private void RestoreWindow()
        {
            // Restaura o estado normal do formulário
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void ExitApplication()
        {
            // Sai da aplicação e limpa o ícone da bandeja
            trayIcon.Dispose();
            Application.Exit();
        }
    }
}
