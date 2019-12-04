
using UnityEngine;

/// <summary>
/// Tile interaction logic for regular (non-reinforced) walls.
/// </summary>
[CreateAssetMenu(fileName = "RegularWallInteraction", menuName = "Interaction/TileInteraction/RegularWallInteraction")]
public class RegularWallInteraction : TileInteraction
{
	public override bool WillInteract(TileApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side)) return false;
		var welder = interaction.HandObject?.GetComponent<Welder>();
		return welder != null && welder.isOn;
	}

	public override void ServerPerformInteraction(TileApply interaction)
	{
		//unweld to a girder
		var progressFinishAction = new ProgressCompleteAction(
			() =>
			{
				SoundManager.PlayNetworkedAtPos("Weld", interaction.WorldPositionTarget, 0.8f);
				interaction.TileChangeManager.RemoveTile(interaction.TargetCellPos, LayerType.Walls);
				SoundManager.PlayNetworkedAtPos("Deconstruct", interaction.WorldPositionTarget, 1f);

				//girder / metal always appears in place of deconstructed wall
				Spawn.ServerPrefab(CommonPrefabs.Instance.Girder, interaction.WorldPositionTarget);
				Spawn.ServerPrefab(CommonPrefabs.Instance.Metal, interaction.WorldPositionTarget);
				interaction.TileChangeManager.SubsystemManager.UpdateAt(interaction.TargetCellPos);
			}
		);

		//Start the progress bar:
		var bar = UIManager.ServerStartProgress(ProgressAction.Construction, interaction.WorldPositionTarget,
			10f, progressFinishAction, interaction.Performer);
		if (bar != null)
		{
			SoundManager.PlayNetworkedAtPos("Weld", interaction.WorldPositionTarget, Random.Range(0.9f, 1.1f));
		}
	}
}
