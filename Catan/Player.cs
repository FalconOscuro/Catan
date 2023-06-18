using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ImGuiNET;

namespace Catan;

class Player
{
    public Player(Board board, Color colour)
    {
        m_GameBoard = board;
        m_TurnState = TurnState.End;
        m_SelectedNode = null;
        Colour = colour;
    }

    public void GiveResource(Resource resource, int num = 1)
    {
        if (resource == Resource.Empty)
            return;
        
        m_Resources[((int)resource)] += num;
    }

    public void StartTurn()
    {
        m_TurnState = TurnState.Start;
    }

    private void Roll()
    {
        m_GameBoard.RollDice();
        m_TurnState = TurnState.Main;
    }

    private void EndTurn()
    {
        m_TurnState = TurnState.End;

        DeselectNode();
    }

    public bool HasTurnEnded()
    {
        return m_TurnState == TurnState.End;
    }

    public void SelectNode(Board.Node node)
    {
        DeselectNode();

        m_SelectedNode = node;
        m_SelectedNode.Selected = true;
    }

    private void DeselectNode()
    {
        if (m_SelectedNode != null)
            m_SelectedNode.Selected = false;

        m_SelectedNode = null;
    }

    public void DrawUI()
    {
        if (ImGui.BeginTable("Resources", 5))
        {
            ImGui.TableNextRow();
            for (int i = 0; i < 5; i++)
            {
                ImGui.TableSetColumnIndex(i);
                ImGui.Text(((Resource)i).ToString());
            }

            ImGui.TableNextRow();
            for (int i = 0; i < 5; i++)
            {
                ImGui.TableSetColumnIndex(i);
                ImGui.Text(m_Resources[i].ToString());
            }
            ImGui.EndTable();
        }

        ImGui.Separator();

        switch(m_TurnState)
        {
            case TurnState.Start:
                TurnStartUI();
                break;
            
            case TurnState.Main:
                TurnMainUI();
                break;
            
            case TurnState.BuildSettlement:
                BuildSettlementUI();
                break;
            
            case TurnState.BuildRoad:
                BuildRoadUI();
                break;
        }
    }

    public void TurnStartUI()
    {
        if (ImGui.Button("Roll"))
            Roll();
    }

    public void TurnMainUI()
    {
        if (ImGui.BeginTabBar("TurnOptions"))
        {
            if (ImGui.BeginTabItem("Build"))
            {
                if (ImGui.Button("Settlement"))
                    m_TurnState = TurnState.BuildSettlement;

                ImGui.SameLine();

                if (ImGui.Button("Road"))
                    m_TurnState = TurnState.BuildRoad;
            }

            if (ImGui.BeginTabItem("Trade"))
            {
                ImGui.Text("Hello there");
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Separator();
        }

        if (ImGui.Button("End Turn"))
            EndTurn();
    }

    private void BuildSettlementUI()
    {
        if (ImGui.Button("Confirm"))
        {
            if (m_SelectedNode != null)
                m_SelectedNode.Owner = this;

            m_TurnState = TurnState.Main;
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
            m_TurnState = TurnState.Main;
    }
    
    private void BuildRoadUI()
    {
        if (ImGui.Button("Confirm"))
            m_TurnState = TurnState.Main;

        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
            m_TurnState = TurnState.Main;
    }

    private enum TurnState {
        PreGame,
        Start,
        Main,
        BuildSettlement,
        BuildRoad,
        End
    }

    public Color Colour { get; private set; }

    private TurnState m_TurnState;

    private int[] m_Resources = new int[] {0, 0, 0, 0, 0};

    private Board m_GameBoard;

    private Board.Node m_SelectedNode;
}