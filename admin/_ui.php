<?php

function paginate($page_var, $end = FALSE) {
	$page = intval(param($page_var, 0));
	$url = $_SERVER["SCRIPT_NAME"];
	$prev_url = $url . '?' . http_build_query(array_merge($_GET, array($page_var => $page - 1)));
	$next_url = $url . '?' . http_build_query(array_merge($_GET, array($page_var => $page + 1)));
	if( $end ) $next_url = NULL;
	if( $page < 1 ) $prev_url = NULL;
?>
<div class="paginate">
	<?php if( $prev_url ): ?>
		<a class="prev" href="<?= $prev_url ?>">&lt; Previous</a>
	<?php endif; ?>

	<?php if( $next_url ): ?>
		<a class="next" href="<?= $next_url ?>">Next &gt;</a>
	<?php endif; ?>
	<div class="clear"></div>
</div>
<?php
}

function table(array $heading, array $data, $format = NULL) {
?>
<table cellpadding="0" cellspacing="0">
	<tr>
		<?php foreach( $heading AS $k => $title ): ?>
		<th><?php echo $title; ?></th>
		<?php endforeach; ?>
	</tr>
<?php foreach( $data AS $row ): ?>
	<tr>
		<?php foreach( $heading AS $k => $title ): ?>
		<td>
			<?php if( isset($format[$k]) ): ?>
				<?php $format_fn = $format[$k]; ?>
				<?= $format_fn($row[$k], $k, $row) ?>
			<?php else: ?>
				<?= $row[$k] ?>
			<?php endif; ?>
		</td>
		<?php endforeach; ?>
	</tr>
<?php endforeach; ?>
</table>
<?php	
}