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
        m_SelectedEdge = null;
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

    public void SetState(TurnState state)
    {
        m_TurnState = state;
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
        DeselectEdge();
    }

    public bool HasTurnEnded()
    {
        return m_TurnState == TurnState.End;
    }

    public void SelectNode(Board.Node node)
    {
        if (m_TurnState == TurnState.End)
            return;

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

    public void SelectEdge(Board.Edge edge)
    {
        if (m_TurnState == TurnState.End)
            return;
        
        DeselectEdge();

        m_SelectedEdge = edge;
        m_SelectedEdge.Selected = true;
    }

    private void DeselectEdge()
    {
        if (m_SelectedEdge != null)
            m_SelectedEdge.Selected = false;
        
        m_SelectedEdge = null;
    }

    public void DrawUI()
    {
        if (m_TurnState == TurnState.PreGame1 || m_TurnState == TurnState.Pregame2)
        {
            PreGameUI();
            return;
        }

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

    private void PreGameUI()
    {
        if (!ImGui.Button("Build") || m_SelectedNode == null || m_SelectedEdge == null)
            return;
            
        else if (!m_SelectedNode.IsAvailable() || !m_SelectedEdge.IsAvailable()
            || (m_SelectedEdge.Nodes[0] != m_SelectedNode && m_SelectedEdge.Nodes[1] != m_SelectedNode))
            return;

        m_SelectedNode.Owner = this;
        m_SelectedEdge.Owner = this;

        if (m_TurnState == TurnState.Pregame2)
            for (int i = 0; i < 3; i++)
                if (m_SelectedNode.Tiles[i] != null)
                    GiveResource(m_SelectedNode.Tiles[i].Type);

        EndTurn();
    }

    private void TurnStartUI()
    {
        if (ImGui.Button("Roll"))
            Roll();
    }

    private void TurnMainUI()
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

    public enum TurnState {
        PreGame1,
        Pregame2,
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
    private Board.Edge m_SelectedEdge;
}