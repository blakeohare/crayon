<?php
  
  function pth_noop() { }
  
	function pth_new_array($size) {
    $output = array();
    while ($size-- > 0) array_push($output, null);
    return array($output);
  }
  
  function pth_dictionary_get_keys($dictionary) {
    $output = array();
    foreach ($dictionary[0] as $key => $ignored) {
      array_push($output, $key);
    }
    return array($output);
  }
  
  $pth_program_data = null;
  function pth_setProgramData($pd) {
    $GLOBALS['pth_program_data'] = $pd;
  }
  
  function pth_getProgramData() {
    return $GLOBALS['pth_program_data'];
  }
  
  function pth_list_reverse($list) {
    $length = count($list[0]);
    for ($i = intval($length / 2) - 1; $i >= 0; --$i) {
      $j = $length - $i - 1;
      $t = $list[0][$i];
      $list[0][$i] = $list[0][$j];
      $list[0][$j] = $t;
    }
  }
  
  function pth_getHttpRequest($stringOut) {
    $stringOut[0][0] = $_SERVER['REQUEST_METHOD'];
    $stringOut[0][1] = $_SERVER['REQUEST_URI'];
    $stringOut[0][2] = $_SERVER['REMOTE_HOST'];
  }
?>